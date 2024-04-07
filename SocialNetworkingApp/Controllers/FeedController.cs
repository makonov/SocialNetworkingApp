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
        private readonly UserManager<User> _userManager;

        public FeedController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext.User;
            int page = 1;
            var user = await _userManager.GetUserAsync(currentUser);
            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, page, 10);

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
            if (!ModelState.IsValid) return View(viewModel);

            var currentUser = HttpContext.User;

            var user = await _userManager.GetUserAsync(currentUser);

            var post = new Post
            {
                UserId = user.Id,
                Image = null,
                Text = viewModel.Text,
                Likes = 0,
                Name = $"{user.LastName} {user.FirstName}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _postRepository.Add(post);
            return RedirectToAction("Index", "Feed");
        }


        [HttpGet]
        public async Task<IActionResult> GetPosts(int page, int lastPostId)
        {
            const int pageSize = 10; // Количество постов на странице


            var currentUser = HttpContext.User;

            var user = await _userManager.GetUserAsync(currentUser);
            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, page, 10, lastPostId);

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
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            var post = await _postRepository.GetByIdAsync(postId);
            var result = _postRepository.Delete(post);
            return result ? Json(new { success = true}) : Json(new {success = false});
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
