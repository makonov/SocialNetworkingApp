using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly IPhotoService _photoService;
        private readonly IImageRepository _imageRepository;
        private readonly IPostRepository _postRepository;
        private readonly UserManager<User> _userManager;
        private readonly IImageAlbumRepository _albumRepository;

        public ImageController(IPhotoService photoService,
            IImageRepository imageRepository,
            IPostRepository postRepository,
            IImageAlbumRepository albumRepository,
            UserManager<User> userManager) 
        {
            _photoService = photoService;
            _imageRepository = imageRepository;
            _postRepository = postRepository;
            _userManager = userManager;
            _albumRepository = albumRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddImage(AddImageViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ошибка при загрузке: изображение не было прикреплено";
                return RedirectToAction("Detail", "Album", new { id = viewModel.ImageAlbumId });
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            string imageDirectory = $"data\\{user.UserName}\\{viewModel.ImageAlbumId}";
            var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Image, imageDirectory);
            string? imagePath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;

            Image image = new Image
            {
                ImageAlbumId = viewModel.ImageAlbumId,
                ImagePath = imagePath,
                Description = viewModel.Description != null ? viewModel.Description : null,
                CreatedAt = DateTime.Now
            };
            _imageRepository.Add(image);

            return RedirectToAction("Detail", "Album", new { id = viewModel.ImageAlbumId });
        }

        public async Task<IActionResult> DeleteImage(int imageId, int albumId)
        {
            try
            {
                var image = await _imageRepository.GetByIdAsync(imageId);

                if (image == null) return NotFound();

                _photoService.DeletePhoto(image.ImagePath);
                _imageRepository.Delete(image);
            }
            finally
            {
                var emptyPosts = await _postRepository.GetAllEmptyAsync();
                emptyPosts.ForEach(p => _postRepository.Delete(p));
            }

            return RedirectToAction("Details", "Album", new { id = albumId });
        }

        [HttpGet]
        public async Task<IActionResult> LoadImages()
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var albums = await _albumRepository.GetAllByUserAsync(user.Id);
            List<int> albumIds = albums.Select(a => a.Id).ToList();
            var images = await _imageRepository.GetAllAsync();
            var imagePaths = images.Where(i => albumIds.Contains(i.ImageAlbumId)).Select(i => i.ImagePath).ToList();
            return Json(new { success = true, data = imagePaths });
        }
    }
}
