using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Controllers;
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
    public class MessengerControllerTests
    {
        private readonly MessengerController _controller;
        private readonly UserManager<User> _userManager;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserService _userService;

        public MessengerControllerTests()
        {
            _userManager = A.Fake<UserManager<User>>();
            _messageRepository = A.Fake<IMessageRepository>();
            _userService = A.Fake<IUserService>();
            _controller = new MessengerController(_userManager, _messageRepository, _userService);
        }

        [Fact]
        public async Task MessengerControllerTests_Index_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task MessengerControllerTests_Index_ShouldReturnView_WhenUserAuthenticated()
        {
            // Arrange
            var currentUser = new User { Id = "1" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _messageRepository.GetLastMessagesForUserAsync(currentUser.Id))
                .Returns(Task.FromResult(new List<Message>()));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task MessengerControllerTests_ShowDialogue_ShouldReturnNotFound_WhenInterlocutorNotExists()
        {
            // Arrange
            var currentUser = new User { Id = "1" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userManager.FindByIdAsync("2"))
                .Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.ShowDialogue("2");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task MessengerControllerTests_ShowDialogue_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.ShowDialogue("2");

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task MessengerControllerTests_ShowDialogue_ShouldReturnView_WhenInterlocutorExists()
        {
            // Arrange
            var currentUser = new User { Id = "1" };
            var interlocutor = new User { Id = "2", FirstName = "Test", LastName = "User" };
            var messages = new List<Message>();

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userManager.FindByIdAsync("2"))
                .Returns(Task.FromResult(interlocutor));
            A.CallTo(() => _messageRepository.GetMessagesByUserIds(currentUser.Id, "2", 1, 10, 0))
                .Returns(Task.FromResult(messages));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.ShowDialogue("2");

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<DialogueViewModel>();
        }

        [Fact]
        public async Task MessengerControllerTests_GetMessages_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetMessages("2", "Test User", 1, 10);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task MessengerControllerTests_GetMessages_ShouldReturnPartialView_WhenUserAuthenticated()
        {
            // Arrange
            var currentUser = new User { Id = "1" };
            var messages = new List<Message> { new Message { Id = 1, Text = "Test" } };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _messageRepository.GetMessagesByUserIds(currentUser.Id, "2", 1, 10, 10))
                .Returns(Task.FromResult(messages));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetMessages("2", "Test User", 1, 10);

            // Assert
            result.Should().BeOfType<PartialViewResult>();
        }
    }

}
