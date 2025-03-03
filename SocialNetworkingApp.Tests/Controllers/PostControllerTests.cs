using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class PostControllerTests
    {
        private readonly PostController _controller;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IPhotoService _photoService;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IProjectFollowerRepository _projectFollowerRepository;
        private readonly ICommunityMemberRepository _communityMemberRepository;
        private readonly UserManager<User> _userManager;

        public PostControllerTests()
        {
            _postRepository = A.Fake<IPostRepository>();
            _likeRepository = A.Fake<ILikeRepository>();
            _friendRepository = A.Fake<IFriendRepository>();
            _photoService = A.Fake<IPhotoService>();
            _albumRepository = A.Fake<IImageAlbumRepository>();
            _imageRepository = A.Fake<IImageRepository>();
            _commentRepository = A.Fake<ICommentRepository>();
            _projectFollowerRepository = A.Fake<IProjectFollowerRepository>();
            _communityMemberRepository = A.Fake<ICommunityMemberRepository>();
            _userManager = A.Fake<UserManager<User>>();

            _controller = new PostController(
                _postRepository, _likeRepository, _friendRepository,
                _photoService, _albumRepository, _imageRepository,
                _commentRepository, _userManager,
                _projectFollowerRepository, _communityMemberRepository);
        }

        [Fact]
        public async Task PostControllerTests_Details_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task PostControllerTests_Details_ShouldReturnViewWithViewModel_WhenUserAuthenticated()
        {
            // Arrange
            var currentUser = new User { Id = "1" };
            var post = new Post { Id = 1 };
            var comments = new List<Comment>();

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _postRepository.GetByIdAsync(1))
                .Returns(Task.FromResult(post));
            A.CallTo(() => _commentRepository.GetByPostIdAsync(1, 1, 10, 0))
                .Returns(Task.FromResult(comments));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<PostCommentsViewModel>();
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldRedirectToIndex_WhenModelStateIsInvalid()
        {
            // Arrange
            var viewModel = new CreatePostViewModel();
            _controller.ModelState.AddModelError("Text", "Required");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));
            var viewModel = new CreatePostViewModel { Text = "Test" };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldAddPostAndRedirectToDetails_WhenUserAuthenticated()
        {
            // Arrange
            var user = new User { Id = "1", FirstName = "John", LastName = "Doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            var viewModel = new CreatePostViewModel { Text = "Test", From = "Project", ProjectId = 1 };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel);

            // Assert
            A.CallTo(() => _postRepository.Add(A<Post>._)).MustHaveHappenedOnceExactly();
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Details");
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldHandleImageUpload_WhenImageProvided()
        {
            // Arrange
            var user = new User { Id = "1", UserName = "john_doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            var viewModel = new CreatePostViewModel { Text = "Test", Image = A.Fake<IFormFile>(), PostTypeId = (int)PostTypeEnum.Profile };

            var fakeAlbum = new ImageAlbum { Id = 1, Name = "Изображения на стене" };
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum> { fakeAlbum }));
            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._)).Returns(Task.FromResult((true, "test.jpg")));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel);

            // Assert
            A.CallTo(() => _imageRepository.Add(A<Image>._)).MustHaveHappenedOnceExactly();
            result.Should().BeOfType<RedirectToActionResult>();
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldSetCommunityId_WhenCommunityIdIsNotNull()
        {
            // Arrange
            var viewModel = new CreatePostViewModel { CommunityId = 1, Text = "Test post" };
            var user = new User { Id = "123" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel);

            // Assert
            A.CallTo(() => _postRepository.Add(A<Post>.That.Matches(p => p.CommunityId == 1))).MustHaveHappened();
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldHandleProjectImageUpload_WhenPostTypeIsProject()
        {
            // Arrange
            var viewModel = new CreatePostViewModel { ProjectId = 1, PostTypeId = (int)PostTypeEnum.Project, Image = A.Fake<IFormFile>() };
            var user = new User { Id = "123" };
            var album = new ImageAlbum { Id = 10, Name = "Изображения на стене" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _albumRepository.GetAllByProjectAsync(1)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._)).Returns(Task.FromResult((true, "image.jpg")));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            await _controller.CreatePost(viewModel);

            // Assert
            A.CallTo(() => _photoService.UploadPhotoAsync(viewModel.Image, "data\\project-1\\10")).MustHaveHappened();
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldHandleCommunityImageUpload_WhenPostTypeIsCommunity()
        {
            // Arrange
            var viewModel = new CreatePostViewModel { CommunityId = 2, PostTypeId = (int)PostTypeEnum.Community, Image = A.Fake<IFormFile>() };
            var user = new User { Id = "123" };
            var album = new ImageAlbum { Id = 20, Name = "Изображения на стене" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _albumRepository.GetAllByCommunityAsync(2)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._)).Returns(Task.FromResult((true, "image.jpg")));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            await _controller.CreatePost(viewModel);

            // Assert
            A.CallTo(() => _photoService.UploadPhotoAsync(viewModel.Image, "data\\community-2\\20")).MustHaveHappened();
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldSetImage_WhenImagePathIsNotNull()
        {
            // Arrange
            var viewModel = new CreatePostViewModel { ImagePath = "path/to/image.jpg", Text = "Test" };
            var user = new User { Id = "123" };
            var image = new Image { ImagePath = "path/to/image.jpg" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _imageRepository.GetByPathAsync("path/to/image.jpg")).Returns(Task.FromResult(image));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            await _controller.CreatePost(viewModel);

            // Assert
            A.CallTo(() => _imageRepository.GetByPathAsync("path/to/image.jpg")).MustHaveHappened();
            A.CallTo(() => _postRepository.Add(A<Post>.That.Matches(p => p.Image == image))).MustHaveHappened();
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldRedirectToCommunityDetails_WhenFromIsCommunity()
        {
            // Arrange
            var viewModel = new CreatePostViewModel { From = "Community", CommunityId = 5, Text = "Test post" };
            var user = new User { Id = "123" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result.ActionName.Should().Be("Details");
            result.ControllerName.Should().Be("Community");
            result.RouteValues["communityId"].Should().Be(5);
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldRedirectToIndex_WhenImagePathIsNull()
        {
            // Arrange
            var viewModel = new CreatePostViewModel { Text = "Test", ImagePath = null };  
            var user = new User { Id = "123", FirstName = "John", LastName = "Doe" };

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task PostControllerTests_CreatePost_ShouldSetImagePathToNull_WhenImageIsInvalid()
        {
            // Arrange
            var user = new User { Id = "123", UserName = "john_doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));

            var viewModel = new CreatePostViewModel
            {
                Text = "Test post",
                Image = A.Fake<IFormFile>(), 
                ImagePath = "invalid/path"
            };

            var imageUploadResult = (IsAttachedAndExtensionValid: false, FileName: "invalid.jpg");

            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._)).Returns(Task.FromResult(imageUploadResult));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreatePost(viewModel);

            // Assert
            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._)).MustHaveHappenedOnceExactly();
            var imagePath = imageUploadResult.IsAttachedAndExtensionValid ? "data\\john_doe" + "\\" + imageUploadResult.FileName : null;
            imagePath.Should().BeNull(); 
        }


        [Fact]
        public async Task PostControllerTests_GetPosts_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetPosts(1, 0);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task PostControllerTests_GetPosts_ShouldReturnPartialView_WhenUserIsAuthenticated()
        {
            // Arrange
            var user = new User { Id = "123", UserName = "john_doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var friendIds = new List<string> { "1", "2" };
            var projectIds = new List<int> {  };
            var communityIds = new List<int> {  };

            A.CallTo(() => _friendRepository.GetAllIdsByUserAsync(user.Id)).Returns(friendIds);
            A.CallTo(() => _projectFollowerRepository.GetAllByUserIdAsync(user.Id)).Returns(Task.FromResult(new List<ProjectFollower>()));
            A.CallTo(() => _communityMemberRepository.GetAllByUserIdAsync(user.Id)).Returns(Task.FromResult(new List<CommunityMember>()));

            var posts = new List<Post>
            {
                new Post { Id = 1, Text = "Post 1", UserId = "1" },
                new Post { Id = 2, Text = "Post 2", UserId = "2" },
                new Post { Id = 2, Text = "Post 2", UserId = "1" },
                new Post { Id = 2, Text = "Post 2" , UserId = "1"}
            };
            A.CallTo(() => _postRepository.GetAllBySubscription(user.Id, friendIds, projectIds, communityIds, 1, 10, 0)).Returns(Task.FromResult(posts));
            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, user.Id)).Returns(true);
            A.CallTo(() => _likeRepository.IsPostLikedByUser(2, user.Id)).Returns(true);

            // Act
            var result = await _controller.GetPosts(1, 0);

            // Assert
            var viewResult = result.Should().BeOfType<PartialViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeAssignableTo<FeedViewModel>().Subject;
            viewModel.Posts.Should().NotBeEmpty(); 
            viewModel.Posts.Count().Should().Be(4); 
            viewModel.CurrentUserId.Should().Be(user.Id);
        }


        [Fact]
        public async Task PostControllerTests_GetPosts_ShouldReturnPartialView_WhenNoPostsFound()
        {
            // Arrange
            var user = new User { Id = "123", UserName = "john_doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));

            // Мокаем возвращаемые данные
            var friendIds = new List<string> { "1", "2" };
            var projectIds = new List<int> { 3, 4 };
            var communityIds = new List<int> { 5, 6 };

            A.CallTo(() => _friendRepository.GetAllIdsByUserAsync(user.Id)).Returns(friendIds);
            A.CallTo(() => _projectFollowerRepository.GetAllByUserIdAsync(user.Id)).Returns(Task.FromResult(new List<ProjectFollower>()));
            A.CallTo(() => _communityMemberRepository.GetAllByUserIdAsync(user.Id)).Returns(Task.FromResult(new List<CommunityMember>()));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Мокаем пустой список постов
            A.CallTo(() => _postRepository.GetAllBySubscription(user.Id, friendIds, projectIds, communityIds, 1, 10, 0))
                .Returns(Task.FromResult(new List<Post>()));

            // Act
            var result = await _controller.GetPosts(1, 0);

            // Assert
            var viewResult = result.Should().BeOfType<PartialViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeAssignableTo<FeedViewModel>().Subject;
            viewModel.Posts.Should().BeEmpty(); // Проверяем, что постов нет
        }

        [Fact]
        public async Task PostControllerTests_GetProfilePosts_ShouldReturnPartialView_WhenUserIsAuthenticated()
        {
            // Arrange
            var user = new User { Id = "123", UserName = "john_doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            
            var posts = new List<Post>
            {
                new Post { Id = 1, Text = "Post 1", UserId = "123" },
                new Post { Id = 2, Text = "Post 2", UserId = "123" }
            };

            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(new Post { UserId = "123" }));
            A.CallTo(() => _postRepository.GetAllFromProfileByUserId(user.Id, 1, 10, 1)).Returns(Task.FromResult(posts));
            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, user.Id)).Returns(true);
            A.CallTo(() => _likeRepository.IsPostLikedByUser(2, user.Id)).Returns(true);

            // Act
            var result = await _controller.GetProfilePosts(1, 1);

            // Assert
            var viewResult = result.Should().BeOfType<PartialViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeAssignableTo<FeedViewModel>().Subject;
            viewModel.Posts.Count().Should().Be(2);
            viewModel.CurrentUserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task PostControllerTests_GetProfilePosts_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetProfilePosts(1, 0);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();  
        }

        [Fact]
        public async Task PostControllerTests_GetProjectPosts_ShouldReturnPartialView_WhenUserIsAuthenticated()
        {
            // Arrange
            var user = new User { Id = "123", UserName = "john_doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var posts = new List<Post>
            {
                new Post { Id = 1, Text = "Project Post 1", ProjectId = 10 },
                new Post { Id = 2, Text = "Project Post 2", ProjectId = 10 }
            };

            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(new Post { ProjectId = 10 }));
            A.CallTo(() => _postRepository.GetAllByProjectId(10, 1, 10, 1)).Returns(Task.FromResult(posts));
            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, user.Id)).Returns(true);
            A.CallTo(() => _likeRepository.IsPostLikedByUser(2, user.Id)).Returns(true);

            // Act
            var result = await _controller.GetProjectPosts(1, 1);

            // Assert
            var viewResult = result.Should().BeOfType<PartialViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeAssignableTo<FeedViewModel>().Subject;
            viewModel.Posts.Count().Should().Be(2);
            viewModel.CurrentUserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task PostControllerTests_GetProjectPosts_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetProjectPosts(1, 0);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task PostControllerTests_GetCommunityPosts_ShouldReturnPartialView_WhenUserIsAuthenticated()
        {
            // Arrange
            var user = new User { Id = "123", UserName = "john_doe" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var posts = new List<Post>
            {
                new Post { Id = 1, Text = "Community Post 1", CommunityId = 5 },
                new Post { Id = 2, Text = "Community Post 2", CommunityId = 5 }
            };

            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(new Post { CommunityId = 5 }));
            A.CallTo(() => _postRepository.GetAllByCommunityId(5, 1, 10, 1)).Returns(Task.FromResult(posts));
            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, user.Id)).Returns(true);
            A.CallTo(() => _likeRepository.IsPostLikedByUser(2, user.Id)).Returns(true);

            // Act
            var result = await _controller.GetCommunityPosts(1, 1);

            // Assert
            var viewResult = result.Should().BeOfType<PartialViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeAssignableTo<FeedViewModel>().Subject;
            viewModel.Posts.Count().Should().Be(2);
            viewModel.CurrentUserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task PostControllerTests_GetCommunityPosts_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetCommunityPosts(1, 0);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }


        [Fact]
        public async Task PostControllerTests_EditPost_ShouldReturnError_WhenPostIsEmpty()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));

            // Act
            var result = await _controller.EditPost(1, null, null, null);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new
                {
                    succsess = false,
                    error = "Пост не может быть пустым"
                });
        }


        [Fact]
        public async Task PostControllerTests_EditPost_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.EditPost(1, "Text");

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldReturnAccessDenied_WhenPostNotFound()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult<Post>(null));

            // Act
            var result = await _controller.EditPost(1, "Text");

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = false, error = "Отказано в доступе" });
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldReturnAccessDenied_WhenUserIsNotOwner()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123" };
            var post = new Post { Id = 1, UserId = "456" }; // Другой владелец
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));

            // Act
            var result = await _controller.EditPost(1, "Text");

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = false, error = "Отказано в доступе" });
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldUpdateText_WhenOnlyTextIsProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123" };
            var post = new Post { Id = 1, UserId = "123", Text = "Old Text" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));

            // Act
            var result = await _controller.EditPost(1, "New Text");

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true });
            post.Text.Should().Be("New Text");
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldUseProjectImageDirectory_WhenProjectIdIsProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var post = new Post { Id = 1, UserId = "123", ProjectId = 10 };
            var inputFile = A.Fake<IFormFile>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));
            var album = new ImageAlbum { Id = 1, Name = "Изображения на стене" };
            A.CallTo(() => _albumRepository.GetAllByProjectAsync(post.ProjectId)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.ReplacePhotoAsync(inputFile, "data\\project-10\\1", null))
                .Returns((true, "newFileName.jpg"));

            // Act
            var result = await _controller.EditPost(post.Id, "New Text", inputFile: inputFile);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, imagePath = "data\\project-10\\1\\newFileName.jpg" });
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldUseCommunityImageDirectory_WhenCommunityIdIsProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var post = new Post { Id = 1, UserId = "123", CommunityId = 20 };
            var inputFile = A.Fake<IFormFile>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));
            var album = new ImageAlbum { Id = 1, Name = "Изображения на стене" };
            A.CallTo(() => _albumRepository.GetAllByCommunityAsync(post.CommunityId)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.ReplacePhotoAsync(inputFile, "data\\community-20\\1", null))
                .Returns((true, "newFileName.jpg"));

            // Act
            var result = await _controller.EditPost(post.Id, "New Text", inputFile: inputFile);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, imagePath = "data\\community-20\\1\\newFileName.jpg" });
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldUseUserImageDirectory_WhenNoProjectOrCommunityIdIsProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var post = new Post { Id = 1, UserId = "123" }; 
            var inputFile = A.Fake<IFormFile>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));
            var album = new ImageAlbum { Id = 1, Name = "Изображения на стене" };
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.ReplacePhotoAsync(inputFile, "data\\testuser\\1", null))
                .Returns((true, "newFileName.jpg"));

            // Act
            var result = await _controller.EditPost(post.Id, "New Text", inputFile: inputFile);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, imagePath = "data\\testuser\\1\\newFileName.jpg" });
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldReturnNullImagePath_WhenImageUploadFails()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var post = new Post { Id = 1, UserId = "123" }; 
            var inputFile = A.Fake<IFormFile>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));
            var album = new ImageAlbum { Id = 1, Name = "Изображения на стене" };
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.ReplacePhotoAsync(inputFile, "data\\testuser\\1", null))
                .Returns((false, "newFileName.jpg"));

            // Act
            var result = await _controller.EditPost(post.Id, "New Text", inputFile: inputFile);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, imagePath = (string)null });
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldPassExistingImagePath_WhenPostImageExists()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var post = new Post { Id = 1, UserId = "123", Image = new Image { ImagePath = "existingImagePath.jpg" } }; 
            var inputFile = A.Fake<IFormFile>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));
            var album = new ImageAlbum { Id = 1, Name = "Изображения на стене" };
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            A.CallTo(() => _photoService.ReplacePhotoAsync(inputFile, "data\\testuser\\1", "existingImagePath.jpg"))
                .Returns((true, "newFileName.jpg"));

            // Act
            var result = await _controller.EditPost(post.Id, "New Text", inputFile: inputFile);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, imagePath = "data\\testuser\\1\\newFileName.jpg" });
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldSetImageFromExistingImage_WhenExistingImageIsProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var existingImagePath = "existingImage.jpg"; 
            var post = new Post { Id = 1, UserId = "123", Image = null }; 
            var inputFile = A.Fake<IFormFile>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));
            var image = new Image { ImagePath = existingImagePath };
            A.CallTo(() => _imageRepository.GetByPathAsync(existingImagePath)).Returns(Task.FromResult(image));

            // Act
            var result = await _controller.EditPost(post.Id, "New Text", existingImage: existingImagePath);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, imagePath = existingImagePath });
            post.Image.Should().BeEquivalentTo(image); 
        }

        [Fact]
        public async Task PostControllerTests_EditPost_ShouldRemoveImage_WhenPostImageExistsAndNoNewImageProvided()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var post = new Post { Id = 1, UserId = "123", Image = new Image { ImagePath = "existingImage.jpg" } }; 
            var inputFile = A.Fake<IFormFile>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(1)).Returns(Task.FromResult(post));

            // Act
            var result = await _controller.EditPost(post.Id, "New Text");

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, imagePath = (string)null });
            post.Image.Should().BeNull(); 
        }

        [Fact]
        public async Task PostControllerTests_LikePost_ShouldReturnUnauthorized_WhenUserIsNotFound()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = (User)null;
            var postId = 1;
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task PostControllerTests_LikePost_ShouldReturnError_WhenPostNotFound()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var postId = 1;
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(postId)).Returns(Task.FromResult<Post>(null)); 

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = false, error = "Пост не найден" });
        }

        [Fact]
        public async Task PostControllerTests_LikePost_ShouldReturnNumberOfLikes_WhenPostIsFoundAndLikeStatusChanged()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "123", UserName = "testuser" };
            var postId = 1;
            var post = new Post { Id = postId, UserId = "123" }; 
            var numberOfLikes = 10;

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _postRepository.GetByIdAsync(postId)).Returns(Task.FromResult(post));
            A.CallTo(() => _likeRepository.ChangeLikeStatus(postId, user.Id)).Returns(Task.FromResult(true));
            A.CallTo(() => _likeRepository.GetNumberOfLikes(postId)).Returns(Task.FromResult(numberOfLikes));

            // Act
            var result = await _controller.LikePost(postId);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true, likes = numberOfLikes });
        }

        [Fact]
        public async Task PostControllerTests_DeletePost_ShouldReturnSuccess_WhenPostIsFound()
        {
            // Arrange
            var postId = 1;
            var post = new Post { Id = postId, Text = "Test post" };
            A.CallTo(() => _postRepository.GetByIdAsync(postId)).Returns(Task.FromResult(post)); 
            A.CallTo(() => _postRepository.Delete(post)).Returns(true); 

            // Act
            var result = await _controller.DeletePost(postId);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task PostControllerTests_DeletePost_ShouldReturnError_WhenPostNotFound()
        {
            // Arrange
            var postId = 1;
            A.CallTo(() => _postRepository.GetByIdAsync(postId)).Returns(Task.FromResult<Post>(null));

            // Act
            var result = await _controller.DeletePost(postId);

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(new { success = false, error = "Пост не найден" });
        }



    }

}
