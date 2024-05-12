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
        private readonly IGifAlbumRepository _albumRepository;
        private readonly IGifRepository _gifRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPhotoService _photoService;
        private readonly IPostRepository _postRepository;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlbumController(IGifAlbumRepository albumRepository, 
            IGifRepository gifRepository, 
            UserManager<User> userManager,
            IPhotoService photoService,
            IPostRepository postRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _albumRepository = albumRepository;
            _gifRepository = gifRepository;
            _userManager = userManager;
            _photoService = photoService;
            _postRepository = postRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            var albums = await _albumRepository.GetAllByUserAsync(user.Id);
            return View(albums);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            GifAlbum album = await _albumRepository.GetByIdAsync(id);
            if (album == null) return NotFound();
            
            var gifs = await _gifRepository.GetByAlbumIdAsync(id);
            GifAlbumViewModel viewModel = new GifAlbumViewModel
            {
                Album = album,
                Gifs = gifs
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

            GifAlbum album = new GifAlbum
            {
                UserId = user.Id,
                Name = viewModel.Title,
                Description = viewModel.Description
            };

            _albumRepository.Add(album);

            if (viewModel.Gif != null)
            {
                string gifDirectory = $"data\\{user.UserName}\\{album.Id}";
                var gifUploadResult = await _photoService.UploadPhotoAsync(viewModel.Gif, gifDirectory);
                string? coverPath = gifUploadResult.IsAttachedAndExtensionValid ? gifDirectory + "\\" + gifUploadResult.FileName : null;
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
                var gifs = await _gifRepository.GetByAlbumIdAsync(id);
                gifs.ForEach(g => _photoService.DeletePhoto(g.GifPath));
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

            GifAlbum? album = await _albumRepository.GetByIdAsync(viewModel.AlbumId);

            if (album != null)
            {
                album.Name = viewModel.Title;
                if (viewModel.Description != null) album.Description = viewModel.Description;
                if (viewModel.Gif != null)
                {
                    string gifDirectory = $"data\\{user.UserName}\\{album.Id}";
                    var gifUploadResult = await _photoService.UploadPhotoAsync(viewModel.Gif, gifDirectory);
                    string? gifPath = gifUploadResult.IsAttachedAndExtensionValid ? gifDirectory + "\\" + gifUploadResult.FileName : null;
                    if (album.CoverPath != null) _photoService.DeletePhoto(album.CoverPath);

                    album.CoverPath = gifPath;
                }
                _albumRepository.Update(album);
                return RedirectToAction("Index");
            }
            TempData["Error"] = $"Альбом не найден";
            return RedirectToAction("Index");
        }

    }
}
