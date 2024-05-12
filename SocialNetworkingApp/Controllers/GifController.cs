using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize]
    public class GifController : Controller
    {
        private readonly IPhotoService _photoService;
        private readonly IGifRepository _gifRepository;
        private readonly IPostRepository _postRepository;
        private readonly UserManager<User> _userManager;
        private readonly IGifAlbumRepository _albumRepository;

        public GifController(IPhotoService photoService, 
            IGifRepository gifRepository,
            IPostRepository postRepository,
            IGifAlbumRepository albumRepository,
            UserManager<User> userManager) 
        {
            _photoService = photoService;
            _gifRepository = gifRepository;
            _postRepository = postRepository;
            _userManager = userManager;
            _albumRepository = albumRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddGif(AddGifViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ошибка при загрузке Gif: изображение не было прикреплено";
                return RedirectToAction("Detail", "Album", new { id = viewModel.GifAlbumId });
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            string gifDirectory = $"data\\{user.UserName}\\{viewModel.GifAlbumId}";
            var gifUploadResult = await _photoService.UploadPhotoAsync(viewModel.Gif, gifDirectory);
            string? gifPath = gifUploadResult.IsAttachedAndExtensionValid ? gifDirectory + "\\" + gifUploadResult.FileName : null;

            Gif gif = new Gif
            {
                GifAlbumId = viewModel.GifAlbumId,
                GifPath = gifPath,
                Description = viewModel.Description != null ? viewModel.Description : null,
                CreatedAt = DateTime.Now
            };
            _gifRepository.Add(gif);

            return RedirectToAction("Detail", "Album", new { id = viewModel.GifAlbumId });
        }

        public async Task<IActionResult> DeleteGif(int gifId, int albumId)
        {
            try
            {
                var gif = await _gifRepository.GetByIdAsync(gifId);

                if (gif == null) return NotFound();

                _photoService.DeletePhoto(gif.GifPath);
                _gifRepository.Delete(gif);
            }
            finally
            {
                var emptyPosts = await _postRepository.GetAllEmptyAsync();
                emptyPosts.ForEach(p => _postRepository.Delete(p));
            }

            return RedirectToAction("Detail", "Album", new { id = albumId });
        }

        [HttpGet]
        public async Task<IActionResult> LoadGifs()
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var albums = await _albumRepository.GetAllByUserAsync(user.Id);
            List<int> albumIds = albums.Select(a => a.Id).ToList();
            var gifs = await _gifRepository.GetAllAsync();
            var gifPaths = gifs.Where(g => albumIds.Contains(g.GifAlbumId)).Select(g => g.GifPath).ToList();
            return Json(new { success = true, data = gifPaths });
        }
    }
}
