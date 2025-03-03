using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Controllers;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class UserReferenceControllerTests
    {
        private readonly UserReferenceController _controller;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStudentGroupRepository _studentGroupRepository;
        private readonly IUserService _userService;

        public UserReferenceControllerTests()
        {
            _userManager = A.Fake<UserManager<User>>();
            _roleManager = A.Fake<RoleManager<IdentityRole>>();
            _studentGroupRepository = A.Fake<IStudentGroupRepository>();
            _userService = A.Fake<IUserService>();

            _controller = new UserReferenceController(
                _userManager,
                _roleManager,
                _studentGroupRepository,
                _userService
            );
        }

        [Fact]
        public async Task UserReferenceController_Index_ShouldReturnView_WithUsers_WhenUsersArePassedAsParameter()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = "user1", UserName = "testUser1", FirstName = "John", LastName = "Doe", IsMale = true },
                new User { Id = "user2", UserName = "testUser2", FirstName = "Jane", LastName = "Smith", IsMale = false }
            };

            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" }
            };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "User" }
            };

            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            A.CallTo(() => _roleManager.Roles).Returns(roles.AsQueryable());

            // Act
            var result = await _controller.Index(users);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as FilterUsersViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Count().Should().Be(2);
            viewModel.Users.First().UserName.Should().Be("testUser1");
            viewModel.Users.Last().UserName.Should().Be("testUser2");
        }

        [Fact]
        public async Task UserReferenceController_Get_CreateUser_ShouldReturnView_WhenCalled()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" }
            };
                var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Student" }
            };

            A.CallTo(() => _roleManager.Roles).Returns(roles.AsQueryable());
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.CreateUser();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as CreateUserViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Groups.Count().Should().Be(1);
            viewModel.AllRoles.Count().Should().Be(1);
        }

        [Fact]
        public async Task UserReferenceController_Get_CreateUser_ShouldReturnRedirect_WhenUserIsAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));

            // Act
            var result = await _controller.CreateUser();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task UserReferenceController_Post_CreateUser_ShouldReturnViewWithError_WhenModelIsInvalid()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var viewModel = new CreateUserViewModel
            {
                UserName = "testUser",
                Password = "password",
                FirstName = "John",
                LastName = "Doe",
                IsMale = true,
                BirthDate = DateTime.Now,
                GroupId = 1
            };
            _controller.ModelState.AddModelError("UserName", "Required");

            // Act
            var result = await _controller.CreateUser(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var returnedViewModel = viewResult.Model as CreateUserViewModel;
            returnedViewModel.Should().BeEquivalentTo(viewModel);
            var errorMessage = _controller.TempData["Error"];
            errorMessage.Should().Be("Ошибка");
        }

        [Fact]
        public async Task UserReferenceController_Post_CreateUser_ShouldReturnViewWithError_WhenUserAlreadyExists()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var viewModel = new CreateUserViewModel
            {
                UserName = "testUser",
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe",
                IsMale = true,
                BirthDate = DateTime.Now,
                GroupId = 1
            };

            A.CallTo(() => _userManager.FindByNameAsync(viewModel.UserName)).Returns(Task.FromResult(new User()));

            // Act
            var result = await _controller.CreateUser(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var returnedViewModel = viewResult.Model as CreateUserViewModel;
            returnedViewModel.Should().BeEquivalentTo(viewModel);
            var errorMessage = _controller.TempData["Error"];
            errorMessage.Should().Be("Пользователь с данным именем уже существует");
        }

        [Fact]
        public async Task UserReferenceController_Post_CreateUser_ShouldReturnViewWithError_WhenPasswordIsInvalid()
        {
            // Arrange
            string userName = "TEST";

            var viewModel = new CreateUserViewModel
            {
                UserName = userName,
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
                UserName = viewModel.UserName,
                GroupId = viewModel.GroupId
            };

            A.CallTo(() => _userManager.CreateAsync(A<User>._, A<string>._)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => _userManager.AddToRoleAsync(A<User>._, UserRoles.User)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => _userManager.FindByNameAsync(A<string>._)).Returns(Task.FromResult<User>(null));

            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Invalid password" })));

            _userManager.PasswordValidators.Add(passwordValidator);



            var roleManager = A.Fake<RoleManager<IdentityRole>>();
            A.CallTo(() => roleManager.RoleExistsAsync(UserRoles.User)).Returns(Task.FromResult(true));

            var signInManager = A.Fake<SignInManager<User>>();
            A.CallTo(() => signInManager.SignInAsync(A<User>._, false, null)).Returns(Task.CompletedTask);


            var albumRepository = A.Fake<IImageAlbumRepository>();
            var groupRepository = A.Fake<IStudentGroupRepository>();

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await _controller.CreateUser(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().BeNull();
            viewResult.Model.Should().BeEquivalentTo(viewModel);

            _controller.TempData["Error"].Should().NotBeNull();
            _controller.TempData["Error"].ToString().Should().Contain("Пароль не соответствует требованиям безопасности");

        }

        [Fact]
        public async Task UserReferenceController_Post_CreateUser_ShouldRedirectWhenRegistrationIsSuccessful()
        {
            // Arrange
            string userName = "TEST";

            var viewModel = new CreateUserViewModel
            {
                UserName = userName,
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
                UserName = viewModel.UserName,
                GroupId = viewModel.GroupId
            };

            A.CallTo(() => _userManager.CreateAsync(A<User>._, A<string>._)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => _userManager.AddToRoleAsync(A<User>._, UserRoles.User)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => _userManager.FindByNameAsync(A<string>._)).Returns(Task.FromResult<User>(null));

            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Success));

            _userManager.PasswordValidators.Add(passwordValidator);


            var roleManager = A.Fake<RoleManager<IdentityRole>>();
            A.CallTo(() => roleManager.RoleExistsAsync(UserRoles.User)).Returns(Task.FromResult(true));

            var signInManager = A.Fake<SignInManager<User>>();
            A.CallTo(() => signInManager.SignInAsync(A<User>._, false, null)).Returns(Task.CompletedTask);


            var albumRepository = A.Fake<IImageAlbumRepository>();
            var groupRepository = A.Fake<IStudentGroupRepository>();

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await _controller.CreateUser(viewModel);

            // Assert
            var viewResult = result as RedirectToActionResult;
            viewResult.Should().NotBeNull();
            viewResult.Should().BeOfType<RedirectToActionResult>();
            viewResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task UserReferenceController_Get_Edit_ShouldReturnView_WhenUserExists()
        {
            // Arrange
            string userId = "user1";
            var user = new User
            {
                Id = userId,
                UserName = "testUser",
                FirstName = "John",
                LastName = "Doe",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };
            var userRoles = new List<string> { "User" };
            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" }
            };

            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult(user));
            A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(Task.FromResult<IList<string>>(userRoles));
            A.CallTo(() => _roleManager.Roles).Returns(new List<IdentityRole> { new IdentityRole { Name = "Admin" }, new IdentityRole { Name = "User" } }.AsQueryable());
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.Edit(userId);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as EditUserViewModel;
            model.Should().NotBeNull();
            model.UserName.Should().Be(user.UserName);
            model.UserID.Should().Be(user.Id);
            model.FirstName.Should().Be(user.FirstName);
            model.LastName.Should().Be(user.LastName);
            model.Groups.Count.Should().Be(1);
            model.Groups.First().Text.Should().Be("Group A");
        }

        [Fact]
        public async Task UserReferenceController_Get_Edit_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            string userId = "invalidUserId";
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Edit(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UserReferenceController_Post_Edit_ShouldRedirectToIndex_WhenUpdateIsSuccessful()
        {
            // Arrange
            var viewModel = new EditUserViewModel
            {
                UserID = "user1",
                UserName = "testUser",
                UserRole = "User",
                FirstName = "John",
                LastName = "Doe",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            var user = new User
            {
                Id = "user1",
                UserName = "testUser",
                FirstName = "John",
                LastName = "Doe",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            A.CallTo(() => _userManager.FindByIdAsync(viewModel.UserID)).Returns(Task.FromResult(user));
            A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(Task.FromResult<IList<string>>(new List<string> { "Admin" }));
            A.CallTo(() => _userManager.RemoveFromRolesAsync(user, A<IEnumerable<string>>._)).Returns(Task.FromResult(new IdentityResult()));
            A.CallTo(() => _userManager.AddToRoleAsync(A<User>._, UserRoles.User)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => _userManager.UpdateAsync(user)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.Edit(viewModel);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task UserReferenceController_Post_Edit_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var viewModel = new EditUserViewModel
            {
                UserID = "invalidUserId",
                UserName = "testUser",
                UserRole = "User",
                FirstName = "John",
                LastName = "Doe",
                GroupId = 1,
                BirthDate = DateTime.Now,
                IsMale = true
            };

            A.CallTo(() => _userManager.FindByIdAsync(viewModel.UserID)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Edit(viewModel);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UserReferenceController_Delete_ShouldReturnView_WhenUserExists()
        {
            // Arrange
            var userId = "user1";
            var user = new User { Id = userId, UserName = "testUser", FirstName = "John", LastName = "Doe" };
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult(user));

            // Act
            var result = await _controller.Delete(userId);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as User;
            viewModel.Should().NotBeNull();
            viewModel.UserName.Should().Be("testUser");
        }

        [Fact]
        public async Task UserReferenceController_Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Delete(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UserReferenceController_DeleteConfirmed_ShouldRedirectToIndex_WhenUserIsDeleted()
        {
            // Arrange
            var userId = "user1";
            var user = new User { Id = userId, UserName = "testUser", FirstName = "John", LastName = "Doe" };
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult(user));
            A.CallTo(() => _userManager.DeleteAsync(user)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.DeleteConfirmed(userId);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task UserReferenceController_DeleteConfirmed_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.DeleteConfirmed(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UserReferenceController_Get_ChangePassword_ShouldReturnView_WhenUserExists()
        {
            // Arrange
            var userId = "user1";
            var user = new User { Id = userId, UserName = "testUser" };

            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult(user));

            // Act
            var result = await _controller.ChangePassword(userId);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ChangePasswordViewModel;
            viewModel.Should().NotBeNull();
            viewModel.UserName.Should().Be(user.UserName);
            viewModel.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task UserReferenceController_Get_ChangePassword_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "invalidUserId";
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.ChangePassword(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UserReferenceController_Post_ChangePassword_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel
            {
                UserId = "user1",
                UserName = "testUser",
                Password = "newPassword",
                ConfirmPassword = "newPassword"
            };
            _controller.ModelState.AddModelError("Password", "Password is required");

            // Act
            var result = await _controller.ChangePassword(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(viewModel);
        }

        [Fact]
        public async Task UserReferenceController_Post_ChangePassword_ShouldReturnViewWithError_WhenPasswordIsInvalid()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel
            {
                UserId = "user1",
                UserName = "testUser",
                Password = "weak",
                ConfirmPassword = "weak"
            };
            var user = new User { Id = "user1", UserName = "testUser" };

            A.CallTo(() => _userManager.FindByIdAsync(viewModel.UserId)).Returns(Task.FromResult(user));
            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Invalid password" })));

            _userManager.PasswordValidators.Add(passwordValidator);

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            // Act
            var result = await _controller.ChangePassword(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeEquivalentTo(viewModel);
            _controller.TempData["Error"].ToString().Should().Contain("Пароль не соответствует требованиям безопасности");
        }

        [Fact]
        public async Task UserReferenceController_Post_ChangePassword_ShouldReturnRedirect_WhenPasswordChangedSuccessfully()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel
            {
                UserId = "user1",
                UserName = "testUser",
                Password = "newPassword123!",
                ConfirmPassword = "newPassword123!"
            };
            var user = new User { Id = "user1", UserName = "testUser" };
            var passwordValidationResult = IdentityResult.Success;
            var resetPasswordResult = IdentityResult.Success;

            A.CallTo(() => _userManager.FindByIdAsync(viewModel.UserId)).Returns(Task.FromResult(user));
            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Success));
            _userManager.PasswordValidators.Add(passwordValidator);
            A.CallTo(() => _userManager.GeneratePasswordResetTokenAsync(user)).Returns(Task.FromResult("token"));
            A.CallTo(() => _userManager.ResetPasswordAsync(user, "token", viewModel.Password))
                .Returns(Task.FromResult(resetPasswordResult));

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await _controller.ChangePassword(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("Index");
            _controller.TempData["Success"].ToString().Should().Contain("Пароль успешно изменен.");
        }

        [Fact]
        public async Task UserReferenceController_Post_ChangePassword_ShouldReturnViewWithError_WhenPasswordChangeFails()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel
            {
                UserId = "user1",
                UserName = "testUser",
                Password = "newPassword123!",
                ConfirmPassword = "newPassword123!"
            };
            var user = new User { Id = "user1", UserName = "testUser" };
            var passwordValidationResult = IdentityResult.Success;
            var resetPasswordResult = IdentityResult.Failed(new IdentityError { Description = "Failed to reset password" });

            A.CallTo(() => _userManager.FindByIdAsync(viewModel.UserId)).Returns(Task.FromResult(user));
            var passwordValidator = A.Fake<IPasswordValidator<User>>();
            A.CallTo(() => passwordValidator.ValidateAsync(A<UserManager<User>>._, A<User>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Success));

            _userManager.PasswordValidators.Add(passwordValidator);
            A.CallTo(() => _userManager.GeneratePasswordResetTokenAsync(user)).Returns(Task.FromResult("token"));
            A.CallTo(() => _userManager.ResetPasswordAsync(user, "token", viewModel.Password))
                .Returns(Task.FromResult(resetPasswordResult));

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = A.Fake<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            // Act
            var result = await _controller.ChangePassword(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _controller.TempData["Error"].ToString().Should().Contain("Не удалось изменить пароль.");
        }

        [Fact]
        public async Task UserReferenceController_Post_ChangePassword_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var viewModel = new ChangePasswordViewModel
            {
                UserId = "nonexistentUser",
                UserName = "testUser",
                Password = "newPassword123!",
                ConfirmPassword = "newPassword123!"
            };

            A.CallTo(() => _userManager.FindByIdAsync(viewModel.UserId)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.ChangePassword(viewModel);

            // Assert
            result.Should().BeOfType<NotFoundResult>();  
        }

        [Fact]
        public async Task UserReferenceController_Filter_ShouldReturnView_WithFilteredUsers_WhenValidFilterIsPassed()
        {
            // Arrange
            var viewModel = new FilterUsersViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                GroupId = 1,
                UserRole = "Admin"
            };

            var users = new List<User>
            {
                new User { Id = "user1", UserName = "testUser1", FirstName = "John", LastName = "Doe", Group = new StudentGroup { GroupName = "Group A" } },
                new User { Id = "user2", UserName = "testUser2", FirstName = "Jane", LastName = "Smith", Group = new StudentGroup { GroupName = "Group B" } }
            };

            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" },
                new StudentGroup { Id = 2, GroupName = "Group B" }
            };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "User" }
            };

            A.CallTo(() => _userService.SearchUsersAsync(viewModel)).Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            A.CallTo(() => _roleManager.Roles).Returns(roles.AsQueryable());

            // Act
            var result = await _controller.Filter(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as FilterUsersViewModel;
            model.Should().NotBeNull();
            model.Users.Count().Should().Be(2);
            model.Users.First().UserName.Should().Be("testUser1");
            model.Users.Last().UserName.Should().Be("testUser2");
            model.Groups.Count().Should().Be(2);
            model.Groups.First().Text.Should().Be("Group A");
            model.Groups.Last().Text.Should().Be("Group B");
        }

        [Fact]
        public async Task UserReferenceController_Filter_ShouldReturnEmptyUserList_WhenNoUsersMatchFilter()
        {
            // Arrange
            var viewModel = new FilterUsersViewModel
            {
                FirstName = "NonExisting",
                LastName = "User",
                GroupId = 1,
                UserRole = "Admin"
            };

            var users = new List<User>(); 

            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" },
                new StudentGroup { Id = 2, GroupName = "Group B" }
            };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "User" }
            };

            A.CallTo(() => _userService.SearchUsersAsync(viewModel)).Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            A.CallTo(() => _roleManager.Roles).Returns(roles.AsQueryable());

            // Act
            var result = await _controller.Filter(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as FilterUsersViewModel;
            model.Should().NotBeNull();
            model.Users.Should().BeEmpty();
        }

        [Fact]
        public async Task UserReferenceController_Filter_ShouldHandleNullUserGroupCorrectly()
        {
            // Arrange
            var viewModel = new FilterUsersViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                GroupId = 1,
                UserRole = "User"
            };

            var users = new List<User>
            {
                new User { Id = "user1", UserName = "testUser1", FirstName = "John", LastName = "Doe", Group = null },
                new User { Id = "user2", UserName = "testUser2", FirstName = "Jane", LastName = "Smith", Group = new StudentGroup { GroupName = "Group A" } }
            };

            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" },
                new StudentGroup { Id = 2, GroupName = "Group B" }
            };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "User" }
            };

            A.CallTo(() => _userService.SearchUsersAsync(viewModel)).Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            A.CallTo(() => _roleManager.Roles).Returns(roles.AsQueryable());

            // Act
            var result = await _controller.Filter(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as FilterUsersViewModel;
            model.Should().NotBeNull();
            model.Users.Count().Should().Be(2);
            model.Users.First().StudentGroup.Should().Be("no info");
            model.Users.Last().StudentGroup.Should().Be("Group A");
        }

        [Fact]
        public async Task UserReferenceController_Filter_ShouldReturnView_WithCorrectRoles_WhenRolesAreFiltered()
        {
            // Arrange
            var viewModel = new FilterUsersViewModel
            {
                UserRole = "Admin"
            };

            var users = new List<User>
            {
                new User { Id = "user1", UserName = "testUser1", FirstName = "John", LastName = "Doe", Group = new StudentGroup { GroupName = "Group A" } },
                new User { Id = "user2", UserName = "testUser2", FirstName = "Jane", LastName = "Smith", Group = new StudentGroup { GroupName = "Group B" } }
            };

            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" },
                new StudentGroup { Id = 2, GroupName = "Group B" }
            };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "User" }
            };

            A.CallTo(() => _userService.SearchUsersAsync(viewModel)).Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            A.CallTo(() => _roleManager.Roles).Returns(roles.AsQueryable());

            // Act
            var result = await _controller.Filter(viewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as FilterUsersViewModel;
            model.Should().NotBeNull();
            model.Users.Count().Should().Be(2);
            model.AllRoles.Should().Contain(role => role.Name == "Admin");
        }

    }

}
