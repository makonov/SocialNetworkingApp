using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private const int PageSize = 10;

        public UserController(IUserService userService, IFriendRequestRepository friendRequestRepository)
        {
            _userService = userService;
            _friendRequestRepository = friendRequestRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersWithFriendStatus(int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var users = await _userService.GetPagedUsers(currentUser.Id, page, PageSize);


            var usersWithFriendStatus = users.Select(u =>
            {
                UserStatus status = UserStatus.None;
                if (_friendRequestRepository.RequestExists(currentUser.Id, u.Id))
                    status = UserStatus.Sender;
                else if (_friendRequestRepository.RequestExists(u.Id, currentUser.Id))
                    status = UserStatus.Reciever;

                return (u, status);
            });

            FindFriendViewModel viewModel = new FindFriendViewModel
            {
                Users = usersWithFriendStatus,
                CurrentUserId = currentUser.Id
            };

            return PartialView("~/Views/Friend/_FindFriendsPartial.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredUsersWithFriendStatus(FindFriendViewModel viewModel, int page = 1)
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

            FindFriendViewModel newViewModel = new FindFriendViewModel
            {
                Users = usersWithFriendStatus,
                CurrentUserId = currentUser.Id
            };

            return PartialView("~/Views/Friend/_FindFriendsPartial.cshtml", newViewModel);
        }
    }
}
