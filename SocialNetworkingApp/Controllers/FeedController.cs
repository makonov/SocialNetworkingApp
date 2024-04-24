using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly UserManager<User> _userManager;
        private const int pageSize = 10;

        public FeedController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            IPhotoService photoService,
            UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _photoService = photoService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext.User;
            int page = 1;
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
                UpdatedAt = DateTime.Now
            };

            if (viewModel.Gif != null)
            {
                string imageDirectory = $"data\\photo\\{user.UserName}";
                var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Gif, imageDirectory);
                post.Gif = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;
            }

            if (viewModel.GifPath != null)
            {
                post.Gif = viewModel.GifPath;
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
            if (post.Gif != null)
            {
                _photoService.DeletePhoto(post.Gif);
            }
            var result = _postRepository.Delete(post);
            return result ? Json(new { success = true}) : Json(new {success = false});
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(int postId, string text, IFormFile inputFile = null)
        {
            if (text == null && inputFile == null)
            {
                return Json(new { succsess = false, error = "Пост не может быть пустым" });
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            var post = await _postRepository.GetByIdAsync(postId);
            if (user.Id == post.UserId)
            {   
                post.UpdatedAt = DateTime.UtcNow;
                post.Text = text;
                
                if (inputFile != null)
                {
                    string imageDirectory = $"data\\photo\\{user.UserName}";
                    var imageUploadResult = await _photoService.ReplacePhotoAsync(inputFile, imageDirectory, post.Gif);
                    post.Gif = imageUploadResult.IsReplacementSuccess ? imageDirectory + "\\" + imageUploadResult.NewFileName : null;
                }
                else
                {
                    _photoService.DeletePhoto(post.Gif);
                    post.Gif = null;
                }
                _postRepository.Update(post);
                return Json(new { success = true, imagePath = post.Gif  });
            }
            return Json(new { success = false, error = "Отказано в доступе" });
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
