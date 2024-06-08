using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;
using System.Net.WebSockets;

namespace SocialNetworkingApp.Controllers
{
    public class FriendController : Controller
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IUserService _userService;
        private readonly IFriendRequestRepository _friendRequestRepository;

        public FriendController(IFriendRepository friendRepository, IUserService userService, IFriendRequestRepository friendRequestRepository)
        {
            _friendRepository = friendRepository;
            _userService = userService;
            _friendRequestRepository = friendRequestRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized();

            var friends = await _friendRepository.GetByUserId(user.Id);
            
            var requests = await _friendRequestRepository.GetRequestsByReceiverId(user.Id);

            FriendsViewModel viewModel = new FriendsViewModel
            {
                Friends = friends,
                CurrentUserId = user.Id,
                Requests = requests
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Find(List<User>? users = null)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized();

            users = await _userService.GetAllUsersExceptCurrentUserAsync(user.Id);

            FindFriendViewModel viewModel = new FindFriendViewModel
            {
                Users = users,
                CurrentUserId = user.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Find(FindFriendViewModel viewModel)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var users = await _userService.FindUsersAsync(viewModel, user.Id);
                FindFriendViewModel newViewModel = new FindFriendViewModel { Users = users };
                return View(newViewModel);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("FromAge", ex.Message);
                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(string userId)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized();

            FriendRequest newRequest = new FriendRequest
            {
                FromUserId = user.Id,
                ToUserId = userId
            };

            try
            {
                _friendRequestRepository.Add(newRequest);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, error = "Произошла ошибка при обработке запроса на добавление в друзья." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DenyRequest(string userId)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized();

            FriendRequest? request = _friendRequestRepository.GetRequest(user.Id, userId);
            if (request == null) return NotFound();

            try
            {
                _friendRequestRepository.Delete(request);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, error = "Произошла ошибка при обработке запроса на добавление в друзья." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(string userId)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (user == null) return Unauthorized();

            FriendRequest? request = _friendRequestRepository.GetRequest(user.Id, userId);
            if (request == null) return NotFound();

            Friend first = new Friend
            {
                FirstUserId = user.Id,
                SecondUserId = userId
            };

            Friend second = new Friend
            {
                FirstUserId = userId,
                SecondUserId = user.Id
            };

            try
            {
                _friendRepository.Add(first);
                _friendRepository.Add(second);
                _friendRequestRepository.Delete(request);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, error = "Произошла ошибка при обработке запроса на добавление в друзья." });
            }
        }
    }
}
