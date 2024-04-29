using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;

namespace SocialNetworkingApp.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IPhotoService _photoService;
        private readonly IGifAlbumRepository _albumRepository;
        private readonly IGifRepository _gifRepository;
        private readonly UserManager<User> _userManager;
        private const int pageSize = 10;

        public FeedController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            IPhotoService photoService,
            IGifAlbumRepository albumRepository,
            IGifRepository gifRepository,
            UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _photoService = photoService;
            _gifRepository = gifRepository;
            _albumRepository = albumRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, page, pageSize);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                return (p, isLikedByCurrentUser);
            });

            var viewModel = new FeedViewModel
            {
                Posts = postsWithLikeStatus,
                CurrentUserId = user.Id
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostViewModel viewModel)
        {
            if (!ModelState.IsValid || (viewModel.Text == null && viewModel.Gif == null && viewModel.GifPath == null))
            {
                return RedirectToAction("Index", "Feed");
            }
            
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);

            var post = new Post
            {
                UserId = user.Id,
                Text = viewModel.Text,
                Likes = 0,
                Name = $"{user.LastName} {user.FirstName}",
                CreatedAt = DateTime.Now,
                UpdatedAt = default
            };

            if (viewModel.Gif != null)
            {
                string imageDirectory = $"data\\photo\\{user.UserName}";
                var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Gif, imageDirectory);
                string gifPath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;

                var gifAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                var album = gifAlbums.FirstOrDefault(g => g.Name == "Gif на стене");
                Gif gif = new Gif
                {
                    GifAlbumId = album.Id,
                    GifPath = gifPath,
                    CreatedAt = DateTime.Now
                };

                
                _gifRepository.Add(gif);
                post.Gif = gif;
            }

            if (viewModel.GifPath != null)
            {
                Gif gif = await _gifRepository.GetByPathAsync(viewModel.GifPath);
                post.Gif = gif;
            }

            _postRepository.Add(post);
            return RedirectToAction("Index", "Feed");
        }


        [HttpGet]
        public async Task<IActionResult> GetPosts(int page, int lastPostId)
        {

            var currentUser = HttpContext.User;

            var user = await _userManager.GetUserAsync(currentUser);
            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, page, pageSize, lastPostId);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                return (p, isLikedByCurrentUser);
            });

            var viewModel = new FeedViewModel
            {
                Posts = postsWithLikeStatus,
                CurrentUserId = user.Id
            };

            return PartialView("_FeedPartial", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(int postId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            var isLiked = await _likeRepository.ChangeLikeStatus(postId, user.Id);
            var post = await _postRepository.GetByIdAsync(postId);
            post.Likes = isLiked ? post.Likes + 1 : post.Likes - 1;
            _postRepository.Update(post);
            return Json(new { success = true, likes = post.Likes });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            var result = _postRepository.Delete(post);
            return result ? Json(new { success = true}) : Json(new {success = false});
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(int postId, string text, string existingGif = null, IFormFile inputFile = null)
        {
            if (text == null && inputFile == null && existingGif == null)
            {
                return Json(new { succsess = false, error = "Пост не может быть пустым" });
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            var post = await _postRepository.GetByIdAsync(postId);
            if (user.Id == post.UserId)
            {   
                post.UpdatedAt = DateTime.Now;
                post.Text = text;
                
                if (inputFile != null)
                {
                    string imageDirectory = $"data\\photo\\{user.UserName}";
                    var imageUploadResult = await _photoService.ReplacePhotoAsync(inputFile, imageDirectory, post.Gif.GifPath);
                    string gifPath = imageUploadResult.IsReplacementSuccess ? imageDirectory + "\\" + imageUploadResult.NewFileName : null;
                    

                    var gifAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                    var album = gifAlbums.FirstOrDefault(g => g.Name == "Gif на стене");
                    Gif gif = new Gif
                    {
                        GifAlbumId = album.Id,
                        GifPath = gifPath,
                        CreatedAt = DateTime.Now
                    };
                    _gifRepository.Add(gif);
                    post.Gif = gif;
                }
                else if (existingGif != null)
                {
                    Gif gif = await _gifRepository.GetByPathAsync(existingGif);
                    post.Gif = gif;
                }
                else if (post.Gif != null)
                {
                    post.Gif = null;
                }
                _postRepository.Update(post);
                return Json(new { success = true, imagePath = post.Gif.GifPath, time = post.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            }
            return Json(new { success = false, error = "Отказано в доступе"});
        }

        [HttpGet]
        public async Task<IActionResult> LoadGifs()
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            var albums = await _albumRepository.GetAllByUserAsync(user.Id);
            List<int> albumIds = albums.Select(a => a.Id).ToList();
            var gifs = await _gifRepository.GetAllAsync();
            var gifPaths = gifs.Where(g => albumIds.Contains(g.GifAlbumId)).Select(g => g.GifPath).ToList();
            return Json(new { success = true, data = gifPaths });
        }

        public IActionResult ShowComments(int page)
        {
            // Здесь вы можете обработать открытие комментариев для поста postId
            return View(page);
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
