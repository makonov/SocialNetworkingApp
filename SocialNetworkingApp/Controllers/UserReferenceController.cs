using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;
using System.Drawing.Printing;
using System.Text.RegularExpressions;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UserReferenceController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStudentGroupRepository _studentGroupRepository;
        private readonly IUserService _userService;

        public UserReferenceController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IStudentGroupRepository studentGroupRepository, IUserService userService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _studentGroupRepository = studentGroupRepository;
            _userService = userService;
        }

        public async Task<IActionResult> Index(IEnumerable<User> users = null)
        {
            if (users == null) users = await _userManager.Users.Include(u => u.Group).ToListAsync();
            var groups = await _studentGroupRepository.GetAllAsync();
            var allRoles = _roleManager.Roles.ToList();
            var userViewModels = new List<UserReferenceViewModel>();
            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                var viewModel = new UserReferenceViewModel
                {
                    UserID = user.Id,
                    UserName = user.UserName,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    IsMale = user.IsMale ?? false,
                    StudentGroup = user.Group != null ? user.Group.GroupName : "no info",
                    BirthDate = user.BirthDate,
                    Roles = roles.ToList(),
                };
                userViewModels.Add(viewModel);
            }

            var userReferenceViewModel = new FilterUsersViewModel
            {
                Users = userViewModels,
                Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList(),
                AllRoles = allRoles
            };

            return View(userReferenceViewModel);
        }

        public async Task<IActionResult> CreateUser()
        {
            var allRoles = _roleManager.Roles.ToList();
            var groups = await _studentGroupRepository.GetAllAsync();
            var response = new CreateUserViewModel
            {
                AllRoles = allRoles,
                Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList()
            };

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel viewModel)
        {
            var groups = await _studentGroupRepository.GetAllAsync();
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ошибка";
                viewModel.AllRoles = _roleManager.Roles.ToList();
                viewModel.Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();
                return View(viewModel);
            }

            // Проверка на существование пользователя
            var user = await _userManager.FindByNameAsync(viewModel.UserName);
            if (user != null)
            {
                TempData["Error"] = "Пользователь с данным именем уже существует";
                viewModel.AllRoles = _roleManager.Roles.ToList();
                viewModel.Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();
                return View(viewModel);
            }

            // Проверка соответствия пароля требованиям
            var passwordValidationResult = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, null, viewModel.Password);
            if (!passwordValidationResult.Succeeded)
            {
                // Пароль не соответствует требованиям
                TempData["Error"] = "Пароль не соответствует требованиям безопасности." +
                    " Минимальная длина пароля - 6 символов, он должен содержать символы " +
                    "нижнего и верхнего регистра, цифры, а также специальные символы.";
                viewModel.AllRoles = _roleManager.Roles.ToList();
                viewModel.Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();
                return View(viewModel);
            }

            var newUser = new User
            {
                UserName = viewModel.UserName,
                LastName = viewModel.LastName,
                FirstName = viewModel.FirstName,
                IsMale = viewModel.IsMale,
                BirthDate = (DateTime) viewModel.BirthDate,
                GroupId = viewModel.GroupId
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, viewModel.Password);

            if (newUserResponse.Succeeded)
                await _userManager.AddToRoleAsync(newUser, viewModel.UserRole);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string userId)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                var groups = await _studentGroupRepository.GetAllAsync();
                EditUserViewModel model = new EditUserViewModel
                {
                    UserName = user.UserName,
                    UserID = user.Id,
                    UserRole = userRoles[0],
                    AllRoles = allRoles,
                    Groups = groups.Select(g => new SelectListItem
                    {
                        Value = g.Id.ToString(),
                        Text = g.GroupName
                    }).ToList(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    GroupId = user.GroupId,
                    BirthDate = user.BirthDate,
                    IsMale = user.IsMale ?? false
            };
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel viewModel)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(viewModel.UserID);
            if (user != null)
            {
                user.BirthDate = viewModel.BirthDate;
                user.FirstName = viewModel.FirstName;
                user.LastName = viewModel.LastName;
                user.IsMale = viewModel.IsMale;
                user.GroupId = viewModel.GroupId;

                await _userManager.UpdateAsync(user);

                return RedirectToAction("Index");
            }

            return NotFound();
        }

        public async Task<IActionResult> Delete(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                return View(user);
            }

            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string Id)
        {
            User? user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ChangePassword(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                ChangePasswordViewModel viewModel = new ChangePasswordViewModel { UserName = user.UserName, UserId = userId };
                return View(viewModel);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            User? user = await _userManager.FindByIdAsync(viewModel.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Проверка соответствия пароля требованиям
            var passwordValidationResult = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, null, viewModel.Password);
            if (!passwordValidationResult.Succeeded)
            {
                // Пароль не соответствует требованиям
                TempData["Error"] = "Пароль не соответствует требованиям безопасности." +
                    " Минимальная длина пароля - 6 символов, он должен содержать символы " +
                    "нижнего и верхнего регистра, цифры, а также специальные символы.";

                return View(viewModel);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, viewModel.Password);

            if (result.Succeeded)
                TempData["Success"] = "Пароль успешно изменен.";
            else
                TempData["Error"] = "Не удалось изменить пароль.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Filter(FilterUsersViewModel viewModel)
        {
            
            var users = await _userService.SearchUsersAsync(viewModel);

            var groups = await _studentGroupRepository.GetAllAsync();
            var allRoles = _roleManager.Roles.ToList();
            var userViewModels = new List<UserReferenceViewModel>();
            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                var model = new UserReferenceViewModel
                {
                    UserID = user.Id,
                    UserName = user.UserName,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    IsMale = user.IsMale ?? false,
                    StudentGroup = user.Group != null ? user.Group.GroupName : "no info",
                    BirthDate = user.BirthDate,
                    Roles = roles.ToList(),
                };
                userViewModels.Add(model);
            }

            var userReferenceViewModel = new FilterUsersViewModel
            {
                Users = userViewModels,
                Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList(),
                AllRoles = allRoles,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                GroupId = viewModel.GroupId,
                BirthDate = viewModel.BirthDate,
                UserRole = viewModel.UserRole
            };

            return View("Index", userReferenceViewModel);
        }
    }
}
