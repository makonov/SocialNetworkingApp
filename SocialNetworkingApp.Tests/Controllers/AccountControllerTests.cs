using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SocialNetworkingApp.Controllers;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly AccountController _controller;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IStudentGroupRepository _groupRepository;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountControllerTests()
        {
            _userManager = A.Fake<UserManager<User>>();
            _signInManager = A.Fake<SignInManager<User>>();
            _albumRepository = A.Fake<IImageAlbumRepository>();
            _groupRepository = A.Fake<IStudentGroupRepository>();
            _roleManager = A.Fake<RoleManager<IdentityRole>>();

            _controller = new AccountController(_userManager, _signInManager, _albumRepository, _groupRepository, _roleManager);
        }

        [Fact]
        public void AccountController_Login_Get_ShouldReturnView()
        {
            // Act
            var result = _controller.Login();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task AccountController_Login_Post_ShouldReturnViewWhenModelStateIsInvalid()
        {
            // Arrange
            var viewModel = new LoginViewModel { Login = "user", Password = "password" };
            _controller.ModelState.AddModelError("Login", "Required");

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task AccountController_Login_Post_ShouldReturnRedirectWhenPasswordIsCorrect()
        {
            // Arrange
            var viewModel = new LoginViewModel { Login = "user", Password = "password" };
            var user = new User { UserName = "user" };

            A.CallTo(() => _userManager.FindByNameAsync(viewModel.Login)).Returns(user);
            A.CallTo(() => _userManager.CheckPasswordAsync(user, viewModel.Password)).Returns(true);

            // Создаем корректный результат для SignInResult
            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;

            A.CallTo(() => _signInManager.PasswordSignInAsync(user, viewModel.Password, false, false)).Returns(Task.FromResult(signInResult));

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }


        [Fact]
        public async Task AccountController_Login_Post_ShouldReturnViewWhenPasswordIsIncorrect()
        {
            // Arrange
            var viewModel = new LoginViewModel { Login = "user", Password = "wrongpassword" };
            var user = new User { UserName = "user" };

            A.CallTo(() => _userManager.FindByNameAsync(viewModel.Login)).Returns(Task.FromResult(user));
            A.CallTo(() => _userManager.CheckPasswordAsync(user, viewModel.Password)).Returns(Task.FromResult(false));

            var tempDataProvider = A.Fake<ITempDataProvider>();
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);

            var accountController = new AccountController(_userManager, _signInManager, _albumRepository, _groupRepository, _roleManager)
            {
                TempData = tempData
            };

            // Act
            var result = await accountController.Login(viewModel);

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            A.CallTo(() => _signInManager.PasswordSignInAsync(user, viewModel.Password, false, false)).MustNotHaveHappened(); 
                                                                                                                              
            tempData["Error"].Should().Be("Неверный пароль. Пожалуйста, попробуйте еще раз.");
        }




        [Fact]
        public async Task AccountController_Register_Get_ShouldReturnViewWithGroups()
        {
            // Arrange
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group1" } };
            A.CallTo(() => _groupRepository.GetAllAsync()).Returns(groups);

            // Act
            var result = await _controller.Register();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<RegisterViewModel>().Subject;
            model.Groups.Count.Should().Be(1);
            model.Groups.First().Text.Should().Be("Group1");
        }

        [Fact]
        public async Task AccountController_Register_Post_ShouldReturnViewWhenModelStateIsInvalid()
        {
            // Arrange
            var viewModel = new RegisterViewModel { Login = "user", Password = "password" };
            _controller.ModelState.AddModelError("Login", "Required");

            // Act
            var result = await _controller.Register(viewModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task AccountController_Register_Post_ShouldReturnViewWhenUserExists()
        {
            // Arrange
            string userName = "TEST";

            var viewModel = new RegisterViewModel
            {
                Login = userName,
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                LastName = "Test",
                FirstName = "Test",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            var existingUser = new User
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                BirthDate = (DateTime)viewModel.BirthDate,
                IsMale = viewModel.IsMale,
                UserName = viewModel.Login,
                GroupId = viewModel.GroupId
            };

            var userManager = A.Fake<UserManager<User>>();
            A.CallTo(() => userManager.FindByNameAsync(A<string>._)).Returns(Task.FromResult(existingUser));

            var roleManager = A.Fake<RoleManager<IdentityRole>>();
            var signInManager = A.Fake<SignInManager<User>>();
            var albumRepository = A.Fake<IImageAlbumRepository>();
            var groupRepository = A.Fake<IStudentGroupRepository>();

            var accountController = new AccountController(userManager, signInManager, albumRepository, groupRepository, roleManager);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            accountController.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await accountController.Register(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().BeNull(); // Ожидаем возврат стандартного View
            viewResult.Model.Should().BeEquivalentTo(viewModel);

            accountController.TempData["Error"].Should().NotBeNull();
            accountController.TempData["Error"].ToString().Should().Contain("Пользователь с данным именем пользователя уже существует");
        }


        [Fact]
        public async Task AccountController_Register_Post_ShouldReturnViewWhenPasswordIsInvalid()
        {
            // Arrange
            string userName = "TEST";

            var viewModel = new RegisterViewModel
            {
                Login = userName,
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                LastName = "Test",
                FirstName = "Test",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            var user = new User
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                BirthDate = (DateTime)viewModel.BirthDate,
                IsMale = viewModel.IsMale,
                UserName = viewModel.Login,
                GroupId = viewModel.GroupId
            };

            var userManager = A.Fake<UserManager<User>>();
            A.CallTo(() => userManager.CreateAsync(A<User>._, A<string>._)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => userManager.AddToRoleAsync(A<User>._, UserRoles.User)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => userManager.FindByNameAsync(A<string>._)).Returns(Task.FromResult<User>(null));

            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Invalid password" })));

            userManager.PasswordValidators.Add(passwordValidator);



            var roleManager = A.Fake<RoleManager<IdentityRole>>();
            A.CallTo(() => roleManager.RoleExistsAsync(UserRoles.User)).Returns(Task.FromResult(true));

            var signInManager = A.Fake<SignInManager<User>>();
            A.CallTo(() => signInManager.SignInAsync(A<User>._, false, null)).Returns(Task.CompletedTask);


            var albumRepository = A.Fake<IImageAlbumRepository>();
            var groupRepository = A.Fake<IStudentGroupRepository>();

            var accountController = new AccountController(userManager, signInManager, albumRepository, groupRepository, roleManager);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            accountController.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await accountController.Register(viewModel);

            // Assert
            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().BeNull();
            viewResult.Model.Should().BeEquivalentTo(viewModel);

            accountController.TempData["Error"].Should().NotBeNull();
            accountController.TempData["Error"].ToString().Should().Contain("Пароль не соответствует требованиям безопасности");

        }


        [Fact]
        public async Task AccountController_Register_Post_ShouldRedirectWhenRegistrationIsSuccessful()
        {
            // Arrange
            string userName = "TEST";

            var viewModel = new RegisterViewModel
            {
                Login = userName,
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                LastName = "Test",
                FirstName = "Test",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            var user = new User
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                BirthDate = (DateTime)viewModel.BirthDate,
                IsMale = viewModel.IsMale,
                UserName = viewModel.Login,
                GroupId = viewModel.GroupId
            };

            var userManager = A.Fake<UserManager<User>>();
            A.CallTo(() => userManager.CreateAsync(A<User>._, A<string>._)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => userManager.AddToRoleAsync(A<User>._, UserRoles.User)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => userManager.FindByNameAsync(A<string>._)).Returns(Task.FromResult<User>(null)); 

            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Success));

            userManager.PasswordValidators.Add(passwordValidator);


            var roleManager = A.Fake<RoleManager<IdentityRole>>();
            A.CallTo(() => roleManager.RoleExistsAsync(UserRoles.User)).Returns(Task.FromResult(true));

            var signInManager = A.Fake<SignInManager<User>>();
            A.CallTo(() => signInManager.SignInAsync(A<User>._, false, null)).Returns(Task.CompletedTask);


            var albumRepository = A.Fake<IImageAlbumRepository>();
            var groupRepository = A.Fake<IStudentGroupRepository>();

            var accountController = new AccountController(userManager, signInManager, albumRepository, groupRepository, roleManager);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            accountController.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await accountController.Register(viewModel);

            // Assert
            var viewResult = result as RedirectToActionResult;
            viewResult.Should().NotBeNull();
            viewResult.Should().BeOfType<RedirectToActionResult>();
            viewResult.ActionName.Should().Be("Index");
        }


        [Fact]
        public async Task AccountController_Logout_ShouldRedirectToLogin()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
        }

        [Fact]
        public async Task AccountController_Login_Post_ShouldReturnViewWhenUserIsNull()
        {
            // Arrange
            string userName = "TEST";

            var viewModel = new LoginViewModel
            {
                Login = userName,
                Password = "Password123!"
            };

            var userManager = A.Fake<UserManager<User>>();
            A.CallTo(() => userManager.FindByNameAsync(A<string>._)).Returns(Task.FromResult<User>(null));

            var signInManager = A.Fake<SignInManager<User>>();

            var accountController = new AccountController(userManager, signInManager, _albumRepository, _groupRepository, _roleManager);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            accountController.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await accountController.Login(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().BeNull(); // Должно вернуть стандартное View
            viewResult.Model.Should().BeEquivalentTo(viewModel);

            accountController.TempData["Error"].Should().NotBeNull();
            accountController.TempData["Error"].ToString().Should().Contain("Пользователь с данным именем пользователя не найден");
        }

        [Fact]
        public async Task AccountController_Login_Post_ShouldReturnViewWhenLoginFails()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Login = "TEST",
                Password = "WrongPassword!"
            };

            var user = new User { UserName = viewModel.Login };

            var userManager = A.Fake<UserManager<User>>();
            A.CallTo(() => userManager.FindByNameAsync(viewModel.Login)).Returns(Task.FromResult(user));
            A.CallTo(() => userManager.CheckPasswordAsync(user, viewModel.Password)).Returns(Task.FromResult(true));

            var signInManager = A.Fake<SignInManager<User>>();
            A.CallTo(() => signInManager.PasswordSignInAsync(user, viewModel.Password, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            var accountController = new AccountController(userManager, signInManager, _albumRepository, _groupRepository, _roleManager);

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            accountController.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await accountController.Login(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(viewModel);

            accountController.TempData["Error"].Should().NotBeNull();
            accountController.TempData["Error"].ToString().Should().Be("Неверный пароль. Пожалуйста, попробуйте еще раз.");
        }

        [Fact]
        public async Task AccountController_Register_Post_ShouldReturnViewWithError_WhenUserCreationFails()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Login = "newUser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                LastName = "Test",
                FirstName = "Test",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            var userManager = A.Fake<UserManager<User>>();
            A.CallTo(() => userManager.FindByNameAsync(viewModel.Login)).Returns(Task.FromResult<User>(null));
            A.CallTo(() => userManager.CreateAsync(A<User>._, A<string>._)).Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Ошибка создания пользователя" })));

            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Success));
            userManager.PasswordValidators.Add(passwordValidator);

            var signInManager = A.Fake<SignInManager<User>>();

            var roleManager = A.Fake<RoleManager<IdentityRole>>();
            var albumRepository = A.Fake<IImageAlbumRepository>();

            var accountController = new AccountController(userManager, signInManager, albumRepository, _groupRepository, roleManager);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            accountController.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await accountController.Register(viewModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(viewModel);

            A.CallTo(() => userManager.CreateAsync(A<User>._, A<string>._)).MustHaveHappenedOnceExactly();
            accountController.TempData["Error"].Should().Be("Произошла ошибка при регистрации пользователя.");
        }

        [Fact]
        public async Task AccountController_Register_Post_ShouldCreateUserRole_WhenRoleDoesNotExist()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Login = "newUser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                LastName = "Test",
                FirstName = "Test",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            var userManager = A.Fake<UserManager<User>>();
            A.CallTo(() => userManager.FindByNameAsync(viewModel.Login)).Returns(Task.FromResult<User>(null));
            A.CallTo(() => userManager.CreateAsync(A<User>._, A<string>._)).Returns(Task.FromResult(IdentityResult.Success));

            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Success));
            userManager.PasswordValidators.Add(passwordValidator);

            var signInManager = A.Fake<SignInManager<User>>();

            var roleManager = A.Fake<RoleManager<IdentityRole>>();
            A.CallTo(() => roleManager.RoleExistsAsync(UserRoles.User)).Returns(Task.FromResult(false));
            A.CallTo(() => roleManager.CreateAsync(A<IdentityRole>._)).Returns(Task.FromResult(IdentityResult.Success));

            var albumRepository = A.Fake<IImageAlbumRepository>();

            var accountController = new AccountController(userManager, signInManager, albumRepository, _groupRepository, roleManager);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            accountController.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await accountController.Register(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Feed");

            A.CallTo(() => roleManager.RoleExistsAsync(UserRoles.User)).MustHaveHappenedOnceExactly();
            A.CallTo(() => roleManager.CreateAsync(A<IdentityRole>._)).MustHaveHappenedOnceExactly();
        }


    }
}
