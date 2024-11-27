using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize]
    public class AlbumController : Controller
    {
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IImageRepository _imageRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPhotoService _photoService;
        private readonly IPostRepository _postRepository;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlbumController(IImageAlbumRepository albumRepository,
            IImageRepository imageRepository, 
            UserManager<User> userManager,
            IPhotoService photoService,
            IPostRepository postRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _albumRepository = albumRepository;
            _imageRepository = imageRepository;
            _userManager = userManager;
            _photoService = photoService;
            _postRepository = postRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string userId = null)
        {
            if (userId == null)
            {
                var currentUser = HttpContext.User;
                var user = await _userManager.GetUserAsync(currentUser);
                userId = user.Id;
            }

            var albums = await _albumRepository.GetAllByUserAsync(userId);
            return View(albums);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            ImageAlbum album = await _albumRepository.GetByIdAsync(id);
            if (album == null) return NotFound();
            
            var images = await _imageRepository.GetByAlbumIdAsync(id);
            ImageAlbumViewModel viewModel = new ImageAlbumViewModel
            {
                Album = album,
                Images = images
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddAlbum(AddAlbumViewModel viewModel)
        {
            if (!ModelState.IsValid) 
            {
                TempData["Error"] = $"Произошла ошибка при создании альбома: {ModelState.GetValueOrDefault("Title")}";
                return RedirectToAction("Index");
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            ImageAlbum album = new ImageAlbum
            {
                UserId = user.Id,
                Name = viewModel.Title,
                Description = viewModel.Description
            };

            _albumRepository.Add(album);

            if (viewModel.Image != null)
            {
                string imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Image, imageDirectory);
                string? coverPath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;
                album.CoverPath = coverPath;
                _albumRepository.Update(album);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteAlbum(int id)
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
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditAlbum(EditAlbumViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = $"Произошла ошибка при редактировании альбома";
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
            }
            TempData["Error"] = $"Альбом не найден";
            return RedirectToAction("Index");
        }

    }
}
