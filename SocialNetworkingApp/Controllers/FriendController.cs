using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System.Net.WebSockets;

namespace SocialNetworkingApp.Controllers
{
    public class FriendController : Controller
    {
        private readonly IFriendRepository _friendRepository;
        private readonly UserManager<User> _userManager;

        public FriendController(IFriendRepository friendRepository, UserManager<User> userManager)
        {
            _friendRepository = friendRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var friends = await _friendRepository.GetByUserId(user.Id);

            FriendsViewModel viewModel = new FriendsViewModel
            {
                Friends = friends,
                CurrentUserId = user.Id
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Find(List<User>? users = null)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            
            if (users == null) users = await _userManager.Users.Where(u => u.Id != user.Id).ToListAsync();
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
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid) return View(viewModel);

            IQueryable<User> users = _userManager.Users;
            if (viewModel.LastName != null)
            {
                string lastName = viewModel.LastName.Trim();
                users = users.Where(u => u.LastName.Contains(lastName));
            }

            if (viewModel.FirstName != null)
            {
                string firstName = viewModel.FirstName.Trim();
                users = users.Where(u => u.FirstName.Contains(firstName));
            }

            if (viewModel.City != null)
            {
                users = users.Where(u => u.City ==  viewModel.City);
            }

            if (viewModel.Gender != null)
            {
                switch(viewModel.Gender)
                {
                    case "Male":
                        users = users.Where(u => u.IsMale == true);
                        break;
                    case "Female":
                        users = users.Where(u => u.IsMale == false);
                        break;
                }
            }

            if (viewModel.FromAge != null && viewModel.ToAge != null && viewModel.FromAge > viewModel.ToAge)
            {
                ModelState.AddModelError("FromAge", "Нижняя граница возраста не может быть больше верхней границы");
                return View(viewModel);
            }

            if (viewModel.FromAge != null) 
            {
                users = users.Where(u => DateTime.Now.Year - u.BirthDate.Year >= viewModel.FromAge);
            }

            if (viewModel.ToAge != null)
            {
                users = users.Where(u => DateTime.Now.Year - u.BirthDate.Year <= viewModel.ToAge);
            }

            var result = await users.Where(u => user.Id != u.Id).ToListAsync();

            FindFriendViewModel newViewModel = new FindFriendViewModel { Users = result };

            return View(newViewModel);
        }
    }
}
