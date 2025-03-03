using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Controllers;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly IUserService _userService;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IStudentGroupRepository _studentGroupRepository;

        public UserControllerTests()
        {
            _userService = A.Fake<IUserService>();
            _friendRequestRepository = A.Fake<IFriendRequestRepository>();
            _studentGroupRepository = A.Fake<IStudentGroupRepository>();

            _controller = new UserController(_userService, _friendRequestRepository, _studentGroupRepository);
        }

        [Fact]
        public async Task UserControllerTests_GetUsersWithFriendStatus_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.GetUsersWithFriendStatus();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task UserControllerTests_GetUsersWithFriendStatus_ShouldReturnUsersWithFriendStatus_WhenUserIsAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var users = new List<User> { new User { Id = "user1", UserName = "User1" }, new User { Id = "user2", UserName = "User2" } };
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group1" }, new StudentGroup { Id = 2, GroupName = "Group2" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, A<string>.Ignored))
                .Returns(true);
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetUsersWithFriendStatus(1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            var viewModel = partialViewResult.Model as FindFriendViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Count().Should().Be(2);
            viewModel.Users.First().Item1.Id.Should().Be("user1");
            viewModel.CurrentUserId.Should().Be(currentUser.Id);
            viewModel.Groups.Count().Should().Be(2);
        }

        [Fact]
        public async Task UserControllerTests_GetUsersWithFriendStatus_ShouldReturnEmptyUsersList_WhenNoUsersExist()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var users = new List<User>();
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetUsersWithFriendStatus(1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            var viewModel = partialViewResult.Model as FindFriendViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Count().Should().Be(0);
        }

        [Fact]
        public async Task UserControllerTests_GetUsersWithFriendStatus_ShouldReturnUsersWithCorrectFriendStatus_WhenRequestsExist()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var users = new List<User> { new User { Id = "user1", UserName = "User1" }, new User { Id = "user2", UserName = "User2" } };
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, "user1"))
                .Returns(true);
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, "user2"))
                .Returns(false);
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetUsersWithFriendStatus(1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            var viewModel = partialViewResult.Model as FindFriendViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Count().Should().Be(2);
            viewModel.Users.First().Item2.Should().Be(UserStatus.Sender);
            viewModel.Users.Last().Item2.Should().Be(UserStatus.None);
        }

        [Fact]
        public async Task UserController_GetUsersWithFriendStatus_ShouldReturnReceiverStatus_WhenRequestExistsForCurrentUser()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "currentUser" };
            var otherUser = new User { Id = "otherUserId", UserName = "otherUser" };
            var users = new List<User> { otherUser };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(otherUser.Id, currentUser.Id))
                .Returns(true); 

            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group 1" } };
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetUsersWithFriendStatus(1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();

            var viewModel = partialViewResult.Model as FindFriendViewModel;
            viewModel.Should().NotBeNull();
            var userStatus = viewModel.Users.First().status;
            userStatus.Should().Be(UserStatus.Reciever);
        }

        [Fact]
        public async Task UserControllerTests_GetFilteredUsersWithFriendStatus_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.GetFilteredUsersWithFriendStatus(new FindFriendViewModel());

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task UserControllerTests_GetFilteredUsersWithFriendStatus_ShouldReturnUsersWithFriendStatus_WhenUserIsAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var users = new List<User> { new User { Id = "user1", UserName = "User1" }, new User { Id = "user2", UserName = "User2" } };
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group1" }, new StudentGroup { Id = 2, GroupName = "Group2" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(A<FindFriendViewModel>.Ignored, currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, "user1"))
                .Returns(true);
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, "user2"))
                .Returns(false);
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetFilteredUsersWithFriendStatus(new FindFriendViewModel(), 1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            var viewModel = partialViewResult.Model as FindFriendViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Count().Should().Be(2);
            viewModel.Users.First().Item2.Should().Be(UserStatus.Sender);
            viewModel.Users.Last().Item2.Should().Be(UserStatus.None);
            viewModel.Groups.Count().Should().Be(2);
        }

        [Fact]
        public async Task UserControllerTests_GetFilteredUsersWithFriendStatus_ShouldReturnEmptyUsersList_WhenNoUsersExist()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var users = new List<User>();
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(A<FindFriendViewModel>.Ignored, currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetFilteredUsersWithFriendStatus(new FindFriendViewModel(), 1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            var viewModel = partialViewResult.Model as FindFriendViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Count().Should().Be(0);
            viewModel.Groups.Count().Should().Be(1);
        }

        [Fact]
        public async Task UserControllerTests_GetFilteredUsersWithFriendStatus_ShouldReturnUsersWithCorrectFriendStatus_WhenRequestsExist()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var users = new List<User> { new User { Id = "user1", UserName = "User1" }, new User { Id = "user2", UserName = "User2" } };
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(A<FindFriendViewModel>.Ignored, currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, "user1"))
                .Returns(true);
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, "user2"))
                .Returns(false);
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetFilteredUsersWithFriendStatus(new FindFriendViewModel(), 1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            var viewModel = partialViewResult.Model as FindFriendViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Count().Should().Be(2);
            viewModel.Users.First().Item2.Should().Be(UserStatus.Sender);
            viewModel.Users.Last().Item2.Should().Be(UserStatus.None);
            viewModel.Groups.Count().Should().Be(1);
        }

        [Fact]
        public async Task UserController_GetFilteredUsersWithFriendStatus_ShouldReturnReceiverStatus_WhenRequestExistsForCurrentUser()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "currentUser" };
            var otherUser = new User { Id = "otherUserId", UserName = "otherUser" };
            var viewModel = new FindFriendViewModel(); 
            var users = new List<User> { otherUser };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(viewModel, currentUser.Id, 1, 10))
                .Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(otherUser.Id, currentUser.Id))
                .Returns(true); // Текущий пользователь - получатель запроса

            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group 1" } };
            A.CallTo(() => _studentGroupRepository.GetAllAsync())
                .Returns(Task.FromResult(groups));

            // Act
            var result = await _controller.GetFilteredUsersWithFriendStatus(viewModel, 1);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();

            var resultModel = partialViewResult.Model as FindFriendViewModel;
            resultModel.Should().NotBeNull();
            var userStatus = resultModel.Users.First().Item2;
            userStatus.Should().Be(UserStatus.Reciever);
        }


    }

}
