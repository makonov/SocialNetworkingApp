using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
    public class ProfileControllerTests
    {
        private readonly ProfileController _controller;
        private readonly IUserService _userService;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IPhotoService _photoService;
        private readonly IImageRepository _imageRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly UserManager<User> _userManager;

        public ProfileControllerTests()
        {
            // Подготовка моков
            _userService = A.Fake<IUserService>();
            _postRepository = A.Fake<IPostRepository>();
            _likeRepository = A.Fake<ILikeRepository>();
            _friendRepository = A.Fake<IFriendRepository>();
            _albumRepository = A.Fake<IImageAlbumRepository>();
            _photoService = A.Fake<IPhotoService>();
            _imageRepository = A.Fake<IImageRepository>();
            _friendRequestRepository = A.Fake<IFriendRequestRepository>();
            _userManager = A.Fake<UserManager<User>>();

            // Инициализация контроллера
            _controller = new ProfileController(
                _postRepository,
                _likeRepository,
                _friendRepository,
                _friendRequestRepository,
                _albumRepository,
                _imageRepository,
                _photoService,
                _userService,
                _userManager
            );
        }

        [Fact]
        public async Task ProfileControllerTests_Index_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null)); 

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProfileControllerTests_Index_ShouldReturnProfileForCurrentUser_WhenUserIdIsNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var profileUser = new User { Id = "currentUserId", UserName = "testUser" };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetUserByIdAsync(currentUser.Id)).Returns(Task.FromResult(profileUser)); 
            A.CallTo(() => _postRepository.GetAllFromProfileByUserId(currentUser.Id, 1, 10, 0)).Returns(Task.FromResult(posts)); 
            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, currentUser.Id)).Returns(true); 

            // Act
            var result = await _controller.Index(userId: null);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ProfileViewModel;
            viewModel.Should().NotBeNull();
            viewModel.User.Id.Should().Be(currentUser.Id);
            viewModel.Posts.Count().Should().Be(1);
        }

        [Fact]
        public async Task ProfileControllerTests_Index_ShouldReturnProfileForOtherUser_WhenUserIdIsProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var profileUser = new User { Id = "otherUserId", UserName = "otherUser" };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetUserByIdAsync("otherUserId")).Returns(Task.FromResult(profileUser)); 
            A.CallTo(() => _postRepository.GetAllFromProfileByUserId("otherUserId", 1, 10, 0)).Returns(Task.FromResult(posts)); 
            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, currentUser.Id)).Returns(false); 

            // Act
            var result = await _controller.Index(userId: "otherUserId");

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ProfileViewModel;
            viewModel.Should().NotBeNull();
            viewModel.User.Id.Should().Be(profileUser.Id);
            viewModel.Posts.Count().Should().Be(1);
        }

        [Fact]
        public async Task ProfileControllerTests_Index_ShouldReturnFriendStatus_WhenUsersAreFriends()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var profileUser = new User { Id = "friendUserId", UserName = "friendUser" };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetUserByIdAsync("friendUserId")).Returns(Task.FromResult(profileUser));
            A.CallTo(() => _postRepository.GetAllFromProfileByUserId("friendUserId", 1, 10, 0)).Returns(Task.FromResult(posts));
            A.CallTo(() => _friendRepository.IsFriend(profileUser.Id, currentUser.Id)).Returns(true); 

            // Act
            var result = await _controller.Index(userId: "friendUserId");

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ProfileViewModel;
            viewModel.Status.Should().Be(UserStatus.Friend); 
        }

        [Fact]
        public async Task ProfileControllerTests_Index_ShouldReturnSenderStatus_WhenUserSentFriendRequest()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var profileUser = new User { Id = "receiverUserId", UserName = "receiverUser" };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetUserByIdAsync("receiverUserId")).Returns(Task.FromResult(profileUser));
            A.CallTo(() => _postRepository.GetAllFromProfileByUserId("receiverUserId", 1, 10, 0)).Returns(Task.FromResult(posts));
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, profileUser.Id)).Returns(true); 

            // Act
            var result = await _controller.Index(userId: "receiverUserId");

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ProfileViewModel;
            viewModel.Status.Should().Be(UserStatus.Sender); 
        }

        [Fact]
        public async Task ProfileControllerTests_Index_ShouldReturnReceiverStatus_WhenUserReceivedFriendRequest()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var profileUser = new User { Id = "senderUserId", UserName = "senderUser" };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetUserByIdAsync("senderUserId")).Returns(Task.FromResult(profileUser));
            A.CallTo(() => _postRepository.GetAllFromProfileByUserId("senderUserId", 1, 10, 0)).Returns(Task.FromResult(posts));
            A.CallTo(() => _friendRequestRepository.RequestExists(profileUser.Id, currentUser.Id)).Returns(true); 

            // Act
            var result = await _controller.Index(userId: "senderUserId");

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ProfileViewModel;
            viewModel.Status.Should().Be(UserStatus.Reciever); 
        }

        [Fact]
        public async Task ProfileControllerTests_ChooseProfilePicture_ShouldUpdateProfilePicture_WhenImageIsSelected()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, Name = "Изображения профиля" };
            var imageUploadResult = (IsAttachedAndExtensionValid: true, FileName: "profilePicture.jpg");
            var fileContent = new byte[] { 0, 1, 2 }; 
            var file = new FormFile(new MemoryStream(fileContent), 0, fileContent.Length, "file", "test.jpg");

            var viewModel = new ProfileViewModel
            {
                Image = file
            };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _albumRepository.GetAllByUserAsync(currentUser.Id)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.UploadPhotoAsync(viewModel.Image, A<string>.Ignored)).Returns(Task.FromResult(imageUploadResult));

            // Act
            var result = await _controller.ChooseProfilePicture(viewModel);

            // Assert
            A.CallTo(() => _imageRepository.Add(A<Image>.Ignored)).MustHaveHappened(); 
            A.CallTo(() => _userManager.UpdateAsync(currentUser)).MustHaveHappened(); 
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index"); 
        }

        [Fact]
        public async Task ProfileControllerTests_ChooseProfilePicture_ShouldUpdateProfilePicture_WhenImagePathIsProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var viewModel = new ProfileViewModel
            {
                ImagePath = "existing/path/to/image.jpg"
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));

            // Act
            var result = await _controller.ChooseProfilePicture(viewModel);

            // Assert
            A.CallTo(() => _userManager.UpdateAsync(currentUser)).MustHaveHappened(); 
            currentUser.ProfilePicture.Should().Be(viewModel.ImagePath); 
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index"); 
        }

        [Fact]
        public async Task ProfileControllerTests_ChooseProfilePicture_ShouldReturnError_WhenNoImageOrImagePathProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>()); 
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var viewModel = new ProfileViewModel();

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));

            // Act
            var result = await _controller.ChooseProfilePicture(viewModel);

            // Assert
            A.CallTo(() => _userManager.UpdateAsync(currentUser)).MustNotHaveHappened();
            var tempData = _controller.TempData["Error"];
            tempData.Should().Be("Произошла ошибка при обновлении фотографии профиля: не выбрано изображение");
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task ProfileControllerTests_ChooseProfilePicture_ShouldReturnUnauthorized_WhenCurrentUserIsNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>()); 
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            var viewModel = new ProfileViewModel();

            // Act
            var result = await _controller.ChooseProfilePicture(viewModel);

            // Assert
            var unauthorizedResult = result as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
        }

        [Fact]
        public async Task ProfileControllerTests_ChooseProfilePicture_ShouldSetImagePathToNull_WhenUploadFails()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>()); 

            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var viewModel = new ProfileViewModel { Image = A.Fake<IFormFile>() };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));

            var imageAlbums = new List<ImageAlbum> { new ImageAlbum { Id = 1, Name = "Изображения профиля" } };
            A.CallTo(() => _albumRepository.GetAllByUserAsync(currentUser.Id)).Returns(Task.FromResult(imageAlbums));

            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>.Ignored, A<string>.Ignored))
                .Returns(Task.FromResult((false, "invalid.jpg")));

            // Act
            var result = await _controller.ChooseProfilePicture(viewModel);

            // Assert
            A.CallTo(() => _userManager.UpdateAsync(currentUser)).MustHaveHappened();
            currentUser.ProfilePicture.Should().BeNull();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");
        }


    }

}
