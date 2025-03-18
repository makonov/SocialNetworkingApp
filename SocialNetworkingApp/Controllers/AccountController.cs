using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System.Text.RegularExpressions;

namespace SocialNetworkingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IStudentGroupRepository _groupRepository;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IImageAlbumRepository albumRepository, IStudentGroupRepository groupRepository, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _albumRepository = albumRepository;
            _groupRepository = groupRepository;
            _roleManager = roleManager;
        }

        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var user = await _userManager.FindByNameAsync(viewModel.Login);

            if (user != null)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, viewModel.Password);
                if (passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Redirect");
                    }
                }
                TempData["Error"] = "Неверный пароль. Пожалуйста, попробуйте еще раз.";
                return View(viewModel);
            }
            TempData["Error"] = "Пользователь с данным именем пользователя не найден. Попробуйте еще раз.";
            return View(viewModel);
        }

        public async Task<IActionResult> Register()
        {
            var groups = await _groupRepository.GetAllAsync();

            var response = new RegisterViewModel
            {
                Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList()
            };

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            var groups = await _groupRepository.GetAllAsync();
            if (!ModelState.IsValid) 
            {
                viewModel.Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();
                return View(viewModel);
            }

            var user = await _userManager.FindByNameAsync(viewModel.Login);
            if (user != null)
            {
                TempData["Error"] = "Пользователь с данным именем пользователя уже существует";
                viewModel.Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();
                return View(viewModel);
            }

            var passwordCheck = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, null, viewModel.Password);
            if (!passwordCheck.Succeeded)
            {
                TempData["Error"] = "Пароль не соответствует требованиям безопасности." +
                    " Минимальная длина пароля - 6 символов, он должен содержать символы " +
                    "нижнего и верхнего регистра, цифры, а также специальные символы.";
                viewModel.Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();
                return View(viewModel);
            }

            var newUser = new User
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                BirthDate = (DateTime) viewModel.BirthDate,
                IsMale = viewModel.IsMale,
                UserName = viewModel.Login,
                GroupId = viewModel.GroupId,
            };

            var result = await _userManager.CreateAsync(newUser, viewModel.Password);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Произошла ошибка при регистрации пользователя.";
                viewModel.Groups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                }).ToList();
                return View(viewModel);
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            await _userManager.AddToRoleAsync(newUser, UserRoles.User);
            await _signInManager.SignInAsync(newUser, isPersistent: false);

            var savedImagesAlbum = new ImageAlbum
            {
                UserId = newUser.Id,
                Name = "Сохраненные изображения",
                IsRequired = true
            };

            var postImages = new ImageAlbum
            {
                UserId = newUser.Id,
                Name = "Изображения на стене",
                IsRequired = true
            };

            var profileImages = new ImageAlbum
            {
                UserId = newUser.Id,
                Name = "Изображения профиля",
                IsRequired = true
            };

            _albumRepository.Add(savedImagesAlbum);
            _albumRepository.Add(postImages);
            _albumRepository.Add(profileImages);

            return RedirectToAction("Index", "Feed");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
