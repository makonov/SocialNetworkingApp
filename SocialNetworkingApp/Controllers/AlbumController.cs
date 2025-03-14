﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class AlbumController : Controller
    {
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IImageRepository _imageRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPhotoService _photoService;
        private readonly IPostRepository _postRepository;
        private readonly IProjectFollowerRepository _followerRepository;
        private readonly ICommunityMemberRepository _communityMemberRepository;
        private readonly ICommunityRepository _communityRepository;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlbumController(IImageAlbumRepository albumRepository,
            IImageRepository imageRepository, 
            UserManager<User> userManager,
            IPhotoService photoService,
            IPostRepository postRepository,
            IProjectFollowerRepository followerRepository,
            ICommunityMemberRepository communityMemberRepository,
            ICommunityRepository communityRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _albumRepository = albumRepository;
            _imageRepository = imageRepository;
            _userManager = userManager;
            _photoService = photoService;
            _postRepository = postRepository;
            _followerRepository = followerRepository;
            _communityMemberRepository = communityMemberRepository;
            _communityRepository = communityRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string userId = null, int? projectId = null, int? communityId = null)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);

            var albums = projectId != null ? await _albumRepository.GetAllByProjectAsync(projectId) : 
                         communityId != null ? await _albumRepository.GetAllByCommunityAsync(communityId) :
                         userId != null && user.Id != userId ? await _albumRepository.GetAllByUserAsync(userId) :
                         await _albumRepository.GetAllByUserAsync(user.Id);

            var viewModel = new AlbumCatalogueViewModel { Albums = albums, CurrentUserId = user.Id };

            if (albums.Count() > 0 && albums.First().ProjectId != null)
            {
                var projectFollower = await _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, (int)projectId);
                if (projectFollower != null && projectFollower.IsMember != null) viewModel.IsProjectMember = true;
                if (projectFollower != null && projectFollower.Project.IsPrivate && !projectFollower.IsMember) viewModel.IsForbidden = true;
            }
            else if (userId == null && albums.Count > 0 && albums.First().CommunityId != null)
            {
                viewModel.IsCommunityAdmin = await _communityMemberRepository.IsAdmin(user.Id, (int)communityId);
                viewModel.IsCommunityMember = await _communityMemberRepository.IsMember(user.Id, (int)communityId);
            }
            else if (userId == null && albums.Count() > 0 && albums.First().UserId == user.Id)
            {
                viewModel.IsOwner = true;
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            ImageAlbum album = await _albumRepository.GetByIdAsync(id);
            if (album == null) return NotFound();
            
            var images = await _imageRepository.GetByAlbumIdAsync(id);
            ImageAlbumViewModel viewModel = new ImageAlbumViewModel
            {
                Album = album,
                Images = images
            };

            if (album.ProjectId != null)
            {
                var projectFollower = await _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, (int)album.ProjectId);
                if (projectFollower != null && projectFollower.IsMember != null) viewModel.IsProjectMember = true;
                if (projectFollower != null && projectFollower.Project.IsPrivate && !projectFollower.IsMember) viewModel.IsForbidden = true;
            } else if (album.CommunityId != null) 
            {
                bool isAdmin = await _communityMemberRepository.IsAdmin(user.Id,(int) album.CommunityId);
                viewModel.IsCommunityAdmin = isAdmin;
            }
            else if (album.UserId != null)
            {
                viewModel.IsOwner = album.UserId == user.Id ? true : false;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddAlbum(AddAlbumViewModel viewModel)
        {
            if (!ModelState.IsValid) 
            {
                TempData["Error"] = $"Произошла ошибка при создании альбома: {ModelState.GetValueOrDefault("Title")}";
                if (viewModel.ProjectId != null) return RedirectToAction("Index", new { projectId = viewModel.ProjectId });
                else return RedirectToAction("Index");
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            ImageAlbum album = new ImageAlbum
            {
                Name = viewModel.Title,
                Description = viewModel.Description
            };

            if (viewModel.ProjectId != null) album.ProjectId = viewModel.ProjectId;
            else if (viewModel.CommunityId != null) album.CommunityId = viewModel.CommunityId;
            else album.UserId = user.Id;

            _albumRepository.Add(album);

            if (viewModel.Image != null)
            {
                string imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Image, imageDirectory);
                string? coverPath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;
                album.CoverPath = coverPath;
                _albumRepository.Update(album);
            }

            if(viewModel.ProjectId != null) return RedirectToAction("Index", new { projectId = viewModel.ProjectId});
            else if (viewModel.CommunityId != null) return RedirectToAction("Index", new { communityId = viewModel.CommunityId });
            else return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteAlbum(int id, int? projectId = null)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);

            if (user == null) return Unauthorized();

            var album = await _albumRepository.GetByIdAsync(id);
            if (album == null) return NotFound();
            try
            {
                _photoService.DeletePhoto(album.CoverPath);
                var images = await _imageRepository.GetByAlbumIdAsync(id);
                images.ForEach(i => _photoService.DeletePhoto(i.ImagePath));
                _albumRepository.Delete(album);
                
                _photoService.DeleteFolder($"data\\{user.UserName}\\{album.Id}");
            }
            catch
            {
                TempData["Error"] = "Произошла ошибка при удалении альбома";
            }

            if (projectId != null) return RedirectToAction("Index", new { projectId = projectId });
            else return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditAlbum(EditAlbumViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = $"Произошла ошибка при редактировании альбома";
                if (viewModel.ProjectId != null) return RedirectToAction("Index", new { projectId = viewModel.ProjectId });
                else return RedirectToAction("Index");
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            ImageAlbum? album = await _albumRepository.GetByIdAsync(viewModel.AlbumId);

            if (album != null)
            {
                album.Name = viewModel.Title;
                if (viewModel.Description != null) album.Description = viewModel.Description;
                if (viewModel.Image != null)
                {
                    string imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                    var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Image, imageDirectory);
                    string? imagePath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;
                    if (album.CoverPath != null) _photoService.DeletePhoto(album.CoverPath);

                    album.CoverPath = imagePath;
                }
                _albumRepository.Update(album);
                if (viewModel.ProjectId != null) return RedirectToAction("Index", new { projectId = viewModel.ProjectId });
                else if (viewModel.CommunityId != null) return RedirectToAction("Index", new { communityId = viewModel.CommunityId });
                else return RedirectToAction("Index");
            }
            TempData["Error"] = $"Альбом не найден";

            if (viewModel.ProjectId != null) return RedirectToAction("Index", new { projectId = viewModel.ProjectId });
            else if (viewModel.CommunityId != null) return RedirectToAction("Index", new { communityId = viewModel.CommunityId });
            else return RedirectToAction("Index");
        }

    }
}
