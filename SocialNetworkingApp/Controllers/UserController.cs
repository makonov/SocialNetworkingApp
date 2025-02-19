using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System.Text.RegularExpressions;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IStudentGroupRepository _studentGroupRepository;
        private const int PageSize = 10;

        public UserController(IUserService userService, IFriendRequestRepository friendRequestRepository, IStudentGroupRepository studentGroupRepository)
        {
            _userService = userService;
            _friendRequestRepository = friendRequestRepository;
            _studentGroupRepository = studentGroupRepository;

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

            var groups = await _studentGroupRepository.GetAllAsync();

            FindFriendViewModel viewModel = new FindFriendViewModel
            {
                Users = usersWithFriendStatus,
                CurrentUserId = currentUser.Id,
                Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList()
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

            var groups = await _studentGroupRepository.GetAllAsync();

            FindFriendViewModel newViewModel = new FindFriendViewModel
            {
                Users = usersWithFriendStatus,
                CurrentUserId = currentUser.Id,
                Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList()
            };

            return PartialView("~/Views/Friend/_FindFriendsPartial.cshtml", newViewModel);
        }
    }
}
