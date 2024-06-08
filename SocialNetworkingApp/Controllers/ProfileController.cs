using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private const int pageSize = 10;

        public ProfileController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            IFriendRequestRepository friendRequestRepository,
            IUserService userService)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _friendRequestRepository = friendRequestRepository;
            _userService = userService;
        }

        // GET: ProfileController
        public async Task<IActionResult> Index(string userId = null, int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            if (userId == null) userId = currentUser.Id;

            var profileUser = await _userService.GetUserByIdAsync(userId);

            var friendIds = new List<string>();
            var posts = await _postRepository.GetAllBySubscription(userId, friendIds, page, pageSize);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, currentUser.Id);
                return (p, isLikedByCurrentUser);
            });

            UserStatus status = UserStatus.None;
            if (userId == currentUser.Id)
            {
                status = UserStatus.Owner;
            }
            else if (_friendRepository.IsFriend(userId, currentUser.Id ))
            {
                status = UserStatus.Friend;
            }
            else if (_friendRequestRepository.RequestExists(currentUser.Id, userId))
            {
                var request = _friendRequestRepository.GetRequest(currentUser.Id, userId);
                status = request.FromUserId == currentUser.Id ? UserStatus.Sender : UserStatus.Reciever;
            }

            var viewModel = new ProfileViewModel
            {
                Posts = postsWithLikeStatus,
                User = profileUser,
                CurrentUserId = currentUser.Id,
                Status = status
            };
            return View(viewModel);
        }

        // GET: ProfileController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProfileController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProfileController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProfileController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProfileController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProfileController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProfileController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
