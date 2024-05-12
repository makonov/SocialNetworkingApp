using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IGifAlbumRepository _albumRepository;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IGifAlbumRepository albumRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _albumRepository = albumRepository;
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

        public IActionResult Register()
        {
            var response = new RegisterViewModel();
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
                City = viewModel.City,
                IsMale = viewModel.IsMale,
                UserName = viewModel.Email,
                Email = viewModel.Email,
            };

            var result = await _userManager.CreateAsync(newUser, viewModel.Password);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Произошла ошибка при регистрации пользователя.";
                return View(viewModel);
            }

            await _userManager.AddToRoleAsync(newUser, UserRoles.User);
            await _signInManager.SignInAsync(newUser, isPersistent: false);

            var savedGifsAlbum = new GifAlbum
            {
                UserId = newUser.Id,
                Name = "Сохраненные Gif",
                IsRequired = true
            };

            var postGifs = new GifAlbum
            {
                UserId = newUser.Id,
                Name = "Gif на стене",
                IsRequired = true
            };

            var profileGifs = new GifAlbum
            {
                UserId = newUser.Id,
                Name = "Gif профиля",
                IsRequired = true
            };

            _albumRepository.Add(savedGifsAlbum);
            _albumRepository.Add(postGifs);
            _albumRepository.Add(profileGifs);

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
