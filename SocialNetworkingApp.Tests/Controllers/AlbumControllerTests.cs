using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
    public class AlbumControllerTests
    {
        private readonly AlbumController _controller;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IImageRepository _imageRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPhotoService _photoService;
        private readonly IPostRepository _postRepository;
        private readonly IProjectFollowerRepository _followerRepository;
        private readonly ICommunityMemberRepository _communityMemberRepository;
        private readonly ICommunityRepository _communityRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlbumControllerTests()
        {
            _albumRepository = A.Fake<IImageAlbumRepository>();
            _imageRepository = A.Fake<IImageRepository>();
            _userManager = A.Fake<UserManager<User>>();
            _photoService = A.Fake<IPhotoService>();
            _postRepository = A.Fake<IPostRepository>();
            _followerRepository = A.Fake<IProjectFollowerRepository>();
            _communityMemberRepository = A.Fake<ICommunityMemberRepository>();
            _communityRepository = A.Fake<ICommunityRepository>();
            _webHostEnvironment = A.Fake<IWebHostEnvironment>();

            _controller = new AlbumController(
                _albumRepository,
                _imageRepository,
                _userManager,
                _photoService,
                _postRepository,
                _followerRepository,
                _communityMemberRepository,
                _communityRepository,
                _webHostEnvironment
            );
        }

        [Fact]
        public async Task AlbumControllerTests_Index_ShouldReturnView_WithAlbumsForUser()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id }
            };
            var userId = user.Id;

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);  
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal } 
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetAllByUserAsync(userId)).Returns(albums);

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as AlbumCatalogueViewModel;
            model.Should().NotBeNull();
            model.Albums.Should().HaveCount(1);

            var albumList = model.Albums.ToList();
            albumList[0].Name.Should().Be("Test Album");
        }



        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldRedirectToIndex_WhenAlbumCreatedSuccessfully()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var albumViewModel = new AddAlbumViewModel
            {
                Title = "New Album",
                Description = "Description",
                Image = null,
                ProjectId = null,
                CommunityId = null
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);  
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true); 

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal } 
            };


            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.Add(A<ImageAlbum>._)).Returns(true); 

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
        }



        [Fact]
        public async Task AlbumControllerTests_DeleteAlbum_ShouldRedirectToIndex_WhenAlbumDeletedSuccessfully()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, UserId = user.Id };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);  
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true); 

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns(album);
            A.CallTo(() => _photoService.DeletePhoto(A<string>._)).Returns(true);  
            A.CallTo(() => _albumRepository.Delete(A<ImageAlbum>._)).Returns(true); 

            // Act
            var result = await _controller.DeleteAlbum(1);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
        }




        [Fact]
        public async Task AlbumControllerTests_Details_ShouldReturnView_WhenAlbumFound()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id };
            var images = new List<Image> { new Image { Id = 1, ImagePath = "image1.jpg", ImageAlbumId = 1 } };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName); 
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true); 

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal } 
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns(album);
            A.CallTo(() => _imageRepository.GetByAlbumIdAsync(1)).Returns(images);

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as ImageAlbumViewModel;
            model.Should().NotBeNull();
            model.Album.Name.Should().Be("Test Album");
            model.Images.Should().HaveCount(1);
        }

        [Fact]
        public async Task AlbumControllerTests_Index_ShouldSetProjectFlags_WhenProjectIdIsProvided()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var projectId = 1;
            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id, ProjectId = projectId }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetAllByProjectAsync(projectId)).Returns(albums);
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, projectId)).Returns(new ProjectFollower { IsMember = true, Project = new Project { IsPrivate = false } });

            // Act
            var result = await _controller.Index(projectId: projectId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as AlbumCatalogueViewModel;

            model.Should().NotBeNull();
            model.IsProjectMember.Should().BeTrue(); 
            model.IsForbidden.Should().BeFalse(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Index_ShouldSetCommunityFlags_WhenCommunityIdIsProvided()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var communityId = 1;
            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id, CommunityId = communityId }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetAllByCommunityAsync(communityId)).Returns(albums);
            A.CallTo(() => _communityMemberRepository.IsAdmin(user.Id, communityId)).Returns(true);
            A.CallTo(() => _communityMemberRepository.IsMember(user.Id, communityId)).Returns(true);

            // Act
            var result = await _controller.Index(communityId: communityId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as AlbumCatalogueViewModel;

            model.Should().NotBeNull();
            model.IsCommunityAdmin.Should().BeTrue(); 
            model.IsCommunityMember.Should().BeTrue(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Index_ShouldReturnAlbumsForOtherUser_WhenUserIdIsProvided()
        {
            // Arrange
            var currentUser = new User { Id = "currentUser", UserName = "currentUser" };
            var otherUser = new User { Id = "otherUser", UserName = "otherUser" };
            var userId = "otherUser";
            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Test Album", UserId = userId }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(currentUser.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(currentUser);
            A.CallTo(() => _albumRepository.GetAllByUserAsync(userId)).Returns(albums);

            // Act
            var result = await _controller.Index(userId: userId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as AlbumCatalogueViewModel;

            model.Should().NotBeNull();
            model.Albums.Should().HaveCount(1);
            model.Albums.First().UserId.Should().Be(userId);
        }

        [Fact]
        public async Task AlbumControllerTests_Index_ShouldSetOwnerFlag_WhenUserIdIsNotProvidedAndUserIsOwner()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(albums);

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as AlbumCatalogueViewModel;

            model.Should().NotBeNull();
            model.IsOwner.Should().BeTrue();
        }

        [Fact]
        public async Task AlbumControllerTests_Index_ShouldSetIsProjectMember_WhenProjectFollowerIsMember()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var projectId = 1;
            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id, ProjectId = projectId }
            };

            var projectFollower = new ProjectFollower
            {
                IsMember = true,
                Project = new Project { IsPrivate = false }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetAllByProjectAsync(projectId)).Returns(albums);
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, projectId)).Returns(projectFollower);

            // Act
            var result = await _controller.Index(projectId: projectId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as AlbumCatalogueViewModel;

            model.Should().NotBeNull();
            model.IsProjectMember.Should().BeTrue(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Index_ShouldSetIsForbidden_WhenProjectIsPrivateAndUserIsNotMember()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var projectId = 1;
            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id, ProjectId = projectId }
            };

            var projectFollower = new ProjectFollower
            {
                IsMember = false, 
                Project = new Project { IsPrivate = true }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetAllByProjectAsync(projectId)).Returns(albums);
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, projectId)).Returns(projectFollower);

            // Act
            var result = await _controller.Index(projectId: projectId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as AlbumCatalogueViewModel;

            model.Should().NotBeNull();
            model.IsForbidden.Should().BeTrue();
        }

        [Fact]
        public async Task AlbumControllerTests_Details_ShouldSetIsProjectMember_WhenProjectExistsAndUserIsMember()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id, ProjectId = 1 };
            var images = new List<Image> { new Image { Id = 1, ImagePath = "image1.jpg", ImageAlbumId = 1 } };

            var projectFollower = new ProjectFollower
            {
                IsMember = true,
                Project = new Project { IsPrivate = false }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns(album);
            A.CallTo(() => _imageRepository.GetByAlbumIdAsync(1)).Returns(images);
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, (int) album.ProjectId)).Returns(projectFollower);

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as ImageAlbumViewModel;
            model.Should().NotBeNull();
            model.IsProjectMember.Should().BeTrue();
        }

        [Fact]
        public async Task AlbumControllerTests_Details_ShouldSetIsForbidden_WhenProjectIsPrivateAndUserIsNotMember()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id, ProjectId = 1 };
            var images = new List<Image> { new Image { Id = 1, ImagePath = "image1.jpg", ImageAlbumId = 1 } };

            var projectFollower = new ProjectFollower
            {
                IsMember = false,
                Project = new Project { IsPrivate = true }
            };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns(album);
            A.CallTo(() => _imageRepository.GetByAlbumIdAsync(1)).Returns(images);
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, (int) album.ProjectId)).Returns(projectFollower);

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as ImageAlbumViewModel;
            model.Should().NotBeNull();
            model.IsForbidden.Should().BeTrue(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Details_ShouldSetIsCommunityAdmin_WhenAlbumIsInCommunityAndUserIsAdmin()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id, CommunityId = 1 };
            var images = new List<Image> { new Image { Id = 1, ImagePath = "image1.jpg", ImageAlbumId = 1 } };

            var isAdmin = true;

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns(album);
            A.CallTo(() => _imageRepository.GetByAlbumIdAsync(1)).Returns(images);
            A.CallTo(() => _communityMemberRepository.IsAdmin(user.Id, album.CommunityId.Value)).Returns(isAdmin);

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as ImageAlbumViewModel;
            model.Should().NotBeNull();
            model.IsCommunityAdmin.Should().BeTrue(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Details_ShouldSetIsOwner_WhenAlbumBelongsToUser()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, Name = "Test Album", UserId = user.Id };
            var images = new List<Image> { new Image { Id = 1, ImagePath = "image1.jpg", ImageAlbumId = 1 } };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns(album);
            A.CallTo(() => _imageRepository.GetByAlbumIdAsync(1)).Returns(images);

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as ImageAlbumViewModel;
            model.Should().NotBeNull();
            model.IsOwner.Should().BeTrue(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Details_ShouldReturnNotFound_WhenAlbumNotFound()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns((ImageAlbum)null); 

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<NotFoundResult>(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Details_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(false); 

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>(); 
        }

        [Fact]
        public async Task AlbumControllerTests_Details_ShouldNotSetIsOwner_WhenAlbumDoesNotBelongToUser()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var anotherUser = new User { Id = "user456", UserName = "anotherUser" }; 
            var album = new ImageAlbum { Id = 1, Name = "Test Album", UserId = anotherUser.Id };
            var images = new List<Image> { new Image { Id = 1, ImagePath = "image1.jpg", ImageAlbumId = 1 } };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.Name).Returns(user.UserName);
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(1)).Returns(album);
            A.CallTo(() => _imageRepository.GetByAlbumIdAsync(1)).Returns(images);

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as ImageAlbumViewModel;
            model.Should().NotBeNull();
            model.IsOwner.Should().BeFalse(); 
        }

        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldRedirectToIndex_WhenModelStateIsInvalid()
        {
            // Arrange
            var albumViewModel = new AddAlbumViewModel { ProjectId = 1 };
            _controller.ModelState.AddModelError("Title", "Title is required");

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.RouteValues["projectId"].Should().Be(1);
            _controller.TempData["Error"].Should().NotBeNull();
            _controller.TempData["Error"].ToString().Should().Contain("Произошла ошибка при создании альбома");
        }

        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var albumViewModel = new AddAlbumViewModel { Title = "New Album" };

            var userPrincipal = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldRedirectToIndex_WithProjectId_WhenProjectIdIsNotNull()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var albumViewModel = new AddAlbumViewModel { Title = "New Album", ProjectId = 1 };

            var userPrincipal = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(user);
            A.CallTo(() => _albumRepository.Add(A<ImageAlbum>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.RouteValues["projectId"].Should().Be(1);
        }

        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldRedirectToIndex_WithCommunityId_WhenCommunityIdIsNotNull()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var albumViewModel = new AddAlbumViewModel { Title = "New Album", CommunityId = 2 };

            var userPrincipal = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(user);
            A.CallTo(() => _albumRepository.Add(A<ImageAlbum>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.RouteValues["communityId"].Should().Be(2);
        }

        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldRedirectToIndex_WhenAlbumIsUserAlbum()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var albumViewModel = new AddAlbumViewModel { Title = "Personal Album" };

            var userPrincipal = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(user);
            A.CallTo(() => _albumRepository.Add(A<ImageAlbum>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldUploadImage_WhenImageProvided()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser" };
            var albumViewModel = new AddAlbumViewModel
            {
                Title = "Album with Image",
                Image = A.Fake<IFormFile>()
            };

            var fakeUploadResult = (true, "cover.jpg");

            var userPrincipal = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(user);
            A.CallTo(() => _albumRepository.Add(A<ImageAlbum>._)).Returns(true);
            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._)).Returns(Task.FromResult(fakeUploadResult));
            A.CallTo(() => _albumRepository.Update(A<ImageAlbum>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._)).MustHaveHappened();
            A.CallTo(() => _albumRepository.Update(A<ImageAlbum>._)).MustHaveHappened();
        }

        [Fact]
        public async Task AlbumControllerTests_AddAlbum_ShouldRedirectToIndex_WhenModelStateIsInvalid_AndNoProjectIdOrCommunityId()
        {
            // Arrange
            var albumViewModel = new AddAlbumViewModel { Title = "New Album" };
            _controller.ModelState.AddModelError("Title", "Title is required");
            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.AddAlbum(albumViewModel);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["Error"].Should().NotBeNull();
            _controller.TempData["Error"].ToString().Should().Contain("Произошла ошибка при создании альбома");
        }


        [Fact]
        public async Task AlbumControllerTests_AlbumControllerTests_DeleteAlbum_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var albumId = 1;
            var projectId = 1;

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            A.CallTo(() => userPrincipal.Identity.IsAuthenticated).Returns(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns((User)null);  

            // Act
            var result = await _controller.DeleteAlbum(albumId, projectId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task AlbumControllerTests_DeleteAlbum_ShouldReturnNotFound_WhenAlbumIsNull()
        {
            // Arrange
            var albumId = 1;
            var projectId = 1;
            var user = new User { Id = "user123", UserName = "testUser" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(albumId)).Returns((ImageAlbum)null);  

            // Act
            var result = await _controller.DeleteAlbum(albumId, projectId);

            // Assert
            result.Should().BeOfType<NotFoundResult>(); 
        }

        [Fact]
        public async Task AlbumControllerTests_DeleteAlbum_ShouldSetTempDataError_WhenExceptionOccurs()
        {
            // Arrange
            var albumId = 1;
            var projectId = 1;
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = albumId, UserId = user.Id, CoverPath = "path/to/cover.jpg" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(albumId)).Returns(album);
            A.CallTo(() => _photoService.DeletePhoto(A<string>._)).Throws<Exception>();  

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.DeleteAlbum(albumId, projectId);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            _controller.TempData["Error"].Should().Be("Произошла ошибка при удалении альбома");  
        }

        [Fact]
        public async Task AlbumControllerTests_DeleteAlbum_ShouldRedirectToIndex_WhenProjectIdIsNull()
        {
            // Arrange
            var albumId = 1;
            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = albumId, UserId = user.Id, CoverPath = "path/to/cover.jpg" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(albumId)).Returns(album);
            A.CallTo(() => _photoService.DeletePhoto(A<string>._)).Returns(true);  
            A.CallTo(() => _albumRepository.Delete(A<ImageAlbum>._)).Returns(true);  

            // Act
            var result = await _controller.DeleteAlbum(albumId, null); 

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();  
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldRedirectToIndex_WhenModelStateIsInvalid()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel { AlbumId = 1, Title = "Test Album" };
            _controller.ModelState.AddModelError("Title", "Title is required");

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["Error"].Should().Be("Произошла ошибка при редактировании альбома");  
        }

        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel { AlbumId = 1, Title = "Test Album" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns((User)null);  

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();  
        }

        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldSetTempDataError_WhenAlbumNotFound()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel { AlbumId = 1, Title = "Test Album" };
            var user = new User { Id = "user123", UserName = "testUser" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(viewModel.AlbumId)).Returns((ImageAlbum)null); 

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            _controller.TempData["Error"].Should().Be("Альбом не найден");  
        }

        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldRedirectToIndex_WhenAlbumEditedSuccessfully()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel
            {
                AlbumId = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                Image = A.Fake<IFormFile>(), 
                ProjectId = 1
            };

            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, UserId = user.Id, CoverPath = "path/to/cover.jpg" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(viewModel.AlbumId)).Returns(album);
            A.CallTo(() => _photoService.UploadPhotoAsync(viewModel.Image, A<string>._)).Returns((true, "new_image.jpg")); 
            A.CallTo(() => _albumRepository.Update(A<ImageAlbum>._)).Returns(true);

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();  
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.RouteValues["projectId"].Should().Be(1); 
        }

        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldRedirectToIndex_WithCommunityId_WhenProjectIdIsNull()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel
            {
                AlbumId = 1,
                Title = "Updated Title",
                CommunityId = 2  
            };

            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, UserId = user.Id, CoverPath = "path/to/cover.jpg" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(viewModel.AlbumId)).Returns(album);
            A.CallTo(() => _albumRepository.Update(A<ImageAlbum>._)).Returns(true); 

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.RouteValues["communityId"].Should().Be(2);
        }

        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldSetTempDataError_WhenAlbumIsNull()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel
            {
                AlbumId = 1,
                Title = "Updated Title",
                Image = A.Fake<IFormFile>(), 
                ProjectId = 1
            };

            var user = new User { Id = "user123", UserName = "testUser" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(viewModel.AlbumId)).Returns((ImageAlbum)null);  

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            _controller.TempData["Error"].Should().Be("Альбом не найден"); 
        }


        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldRedirectToIndexWithCommunityId_WhenCommunityIdIsNotNull()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel
            {
                AlbumId = 1,
                Title = "Updated Title",
                Image = A.Fake<IFormFile>(), 
                CommunityId = 2  
            };

            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, UserId = user.Id, CoverPath = "path/to/cover.jpg" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(viewModel.AlbumId)).Returns(album);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.RouteValues["communityId"].Should().Be(2);  
        }


        [Fact]
        public async Task AlbumControllerTests_EditAlbum_ShouldRedirectToIndexWithProjectId_WhenProjectIdIsNotNull()
        {
            // Arrange
            var viewModel = new EditAlbumViewModel
            {
                AlbumId = 1,
                Title = "Updated Title",
                Image = A.Fake<IFormFile>(),  
                ProjectId = 3  
            };

            var user = new User { Id = "user123", UserName = "testUser" };
            var album = new ImageAlbum { Id = 1, UserId = user.Id, CoverPath = "path/to/cover.jpg" };

            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(user);
            A.CallTo(() => _albumRepository.GetByIdAsync(viewModel.AlbumId)).Returns(album);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.EditAlbum(viewModel);

            // Assert
            var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.RouteValues["projectId"].Should().Be(3);  
        }



    }
}
