using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IStudentGroupRepository _groupRepository;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IImageAlbumRepository albumRepository, IStudentGroupRepository groupRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _albumRepository = albumRepository;
            _groupRepository = groupRepository;

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

            var user = await _userManager.FindByEmailAsync(viewModel.Login);

            if (user != null)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, viewModel.Password);
                if (passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Feed");
                    }
                }
                TempData["Error"] = "Неверный пароль. Пожалуйста, попробуйте еще раз.";
                return View(viewModel);
            }
            TempData["Error"] = "Пользователь с данным электронным адресом не найден. Попробуйте еще раз.";
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
            if (!ModelState.IsValid) return View(viewModel);

            var user = await _userManager.FindByEmailAsync(viewModel.Email);
            if (user != null)
            {
                TempData["Error"] = "Пользователь с данным электронным адресом уже существует";
                return View(viewModel);
            }

            var passwordCheck = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, null, viewModel.Password);
            if (!passwordCheck.Succeeded)
            {
                TempData["Error"] = "Пароль не соответствует требованиям безопасности." +
                    " Минимальная длина пароля - 6 символов, он должен содержать символы " +
                    "нижнего и верхнего регистра, цифры, а также специальные символы.";
                return View(viewModel);
            }

            var newUser = new User
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                BirthDate = (DateTime) viewModel.BirthDate,
                IsMale = viewModel.IsMale,
                UserName = viewModel.Email,
                Email = viewModel.Email,
                GroupId = viewModel.GroupId,
                RoleId = (int) UserRoleEnum.Student
            };

            var result = await _userManager.CreateAsync(newUser, viewModel.Password);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Произошла ошибка при регистрации пользователя.";
                return View(viewModel);
            }

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
