using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class FriendController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IFriendRepository _friendRepository;
        private readonly IUserService _userService;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IStudentGroupRepository _studentGroupRepository;
        private const int PageSize = 10;

        public FriendController(UserManager<User> userManager, 
            IFriendRepository friendRepository, 
            IUserService userService, 
            IFriendRequestRepository friendRequestRepository,
            IStudentGroupRepository studentGroupRepository)
        {
            _friendRepository = friendRepository;
            _userService = userService;
            _friendRequestRepository = friendRequestRepository;
            _userManager = userManager;
            _studentGroupRepository = studentGroupRepository;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var friends = await _friendRepository.GetByUserId(currentUser.Id, page, PageSize);
            
            var requests = await _friendRequestRepository.GetRequestsByReceiverId(currentUser.Id);

            FriendsViewModel viewModel = new FriendsViewModel
            {
                Friends = friends,
                CurrentUserId = currentUser.Id,
                Requests = requests
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GetFriends(int page, int lastFriendId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var friends = await _friendRepository.GetByUserId(currentUser.Id, page, PageSize, lastFriendId);

            FriendsViewModel viewModel = new FriendsViewModel
            {
                Friends = friends,
                CurrentUserId = currentUser.Id,
            };

            return PartialView("~/Views/Friend/_ShowFriendsPartial.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> FindFiltered(FindFriendViewModel viewModel = null, int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var users = await _userService.FindUsersPagedAsync(viewModel, currentUser.Id, page, PageSize);

            var usersWithFriendStatus = users.Select(u =>
            {
                UserStatus status = UserStatus.None;
                if (_friendRequestRepository.RequestExists(currentUser.Id, u.Id))
                    status = UserStatus.Sender;
                else if (_friendRequestRepository.RequestExists(u.Id, currentUser.Id))
                    status = UserStatus.Reciever;

                return (u, status);
            });

            var groups = await _studentGroupRepository.GetAllAsync();

            FindFriendViewModel newViewModel = new FindFriendViewModel
            {
                Users = usersWithFriendStatus,
                CurrentUserId = currentUser.Id,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                StudentGroupId = viewModel.StudentGroupId,
                Gender = viewModel.Gender,
                FromAge = viewModel.FromAge,
                ToAge = viewModel.ToAge,
                Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList()
            };

            return View(newViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Find(List<User>? users = null, int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            users = await _userService.GetPagedUsers(currentUser.Id, page, PageSize);

            var usersWithFriendStatus = users.Select(u =>
            {
                UserStatus status = UserStatus.None;
                if (_friendRequestRepository.RequestExists(currentUser.Id, u.Id))
                    status = UserStatus.Sender;
                else if (_friendRequestRepository.RequestExists(u.Id, currentUser.Id))
                    status = UserStatus.Reciever;
                else if (_friendRepository.IsFriend(u.Id, currentUser.Id))
                    status = UserStatus.Friend;

                return (u, status);
            });

            var groups = await _studentGroupRepository.GetAllAsync();

            FindFriendViewModel viewModel = new FindFriendViewModel
            {
                Users = usersWithFriendStatus,
                CurrentUserId = currentUser.Id,
                Groups =  groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Find(FindFriendViewModel viewModel, int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var groups = (await _studentGroupRepository.GetAllAsync()).Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.GroupName
            }).ToList();
            viewModel.Groups = groups;

            if (!ModelState.IsValid) return View(viewModel);

            try
            {

                FindFriendViewModel newViewModel = new FindFriendViewModel
                {
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    StudentGroupId = viewModel.StudentGroupId,
                    Gender = viewModel.Gender,
                    FromAge = viewModel.FromAge,
                    ToAge = viewModel.ToAge,
                    Groups = groups
                };
                return RedirectToAction("FindFiltered", newViewModel);
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
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            FriendRequest newRequest = new FriendRequest
            {
                FromUserId = currentUser.Id,
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
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            FriendRequest? request = _friendRequestRepository.GetRequest(currentUser.Id, userId);
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
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            FriendRequest? request = _friendRequestRepository.GetRequest(currentUser.Id, userId);
            if (request == null) return NotFound();

            Friend firstRelation = new Friend
            {
                FirstUserId = currentUser.Id,
                SecondUserId = userId
            };

            Friend secondRelation = new Friend
            {
                FirstUserId = userId,
                SecondUserId = currentUser.Id
            };

            try
            {
                User? friend = await _userManager.FindByIdAsync(userId);
                if (friend == null) return NotFound();

                _friendRepository.Add(firstRelation);
                _friendRepository.Add(secondRelation);
                _friendRequestRepository.Delete(request);
                return Json(new { success = true, friendId = friend.Id, FirstName = friend.FirstName, LastName = friend.LastName, ProfilePicture = friend.ProfilePicture});
            }
            catch
            {
                return Json(new { success = false, error = "Произошла ошибка при обработке запроса на добавление в друзья." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFriend(string userId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            Friend? firstRelation = await _friendRepository.GetByUserId(userId, currentUser.Id);
            Friend? secondRelation = await _friendRepository.GetByUserId(currentUser.Id, userId);

            if (firstRelation == null || secondRelation == null) return NotFound();

            try
            {
                _friendRepository.Delete(firstRelation);
                _friendRepository.Delete(secondRelation);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, error = "Произошла ошибка при удалении друга." });
            }
        }
    }
}
