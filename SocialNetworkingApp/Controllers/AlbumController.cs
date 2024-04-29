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
    public class AlbumController : Controller
    {
        private readonly IGifAlbumRepository _albumRepository;
        private readonly IGifRepository _gifRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPhotoService _photoService;
        private readonly IPostRepository _postRepository;

        public AlbumController(IGifAlbumRepository albumRepository, 
            IGifRepository gifRepository, 
            UserManager<User> userManager,
            IPhotoService photoService,
            IPostRepository postRepository)
        {
            _albumRepository = albumRepository;
            _gifRepository = gifRepository;
            _userManager = userManager;
            _photoService = photoService;
            _postRepository = postRepository;
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
            var gifs = await _gifRepository.GetByAlbumIdAsync(id);
            GifAlbumViewModel viewModel = new GifAlbumViewModel
            {
                Album = album,
                Gifs = gifs
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddGif(AddGifViewModel viewModel)
        {
            if (!ModelState.IsValid) {
                TempData["Error"] = "Ошибка при загрузке Gif: изображение не было прикреплено";
                return RedirectToAction("Detail", new { id = viewModel.GifAlbumId });
            }
          
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            string imageDirectory = $"data\\photo\\{user.UserName}";
            var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Gif, imageDirectory);
            string gifPath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;

            Gif gif = new Gif
            {
                GifAlbumId = viewModel.GifAlbumId,
                GifPath = gifPath,
                CreatedAt = DateTime.Now
            };
            _gifRepository.Add(gif);

            return RedirectToAction("Detail", new {id = viewModel.GifAlbumId});
        }

        public async Task<IActionResult> DeleteGif(int gifId, int albumId)
        {
            try
            {
                var gif = await _gifRepository.GetByIdAsync(gifId);

                if (gif == null)
                {
                    return NotFound();
                }

                _photoService.DeletePhoto(gif.GifPath);
                _gifRepository.Delete(gif);
            }
            finally
            {
                var emptyPosts = await _postRepository.GetAllEmptyAsync();
                emptyPosts.ForEach(p => _postRepository.Delete(p));
            }

            return RedirectToAction("Detail", new { id = albumId });
        }

    }
}
