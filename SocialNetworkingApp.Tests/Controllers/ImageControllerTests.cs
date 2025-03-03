using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using SocialNetworkingApp.Controllers;
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
    public class ImageControllerTests
    {
        private readonly ImageController _controller;
        private readonly IPhotoService _photoService;
        private readonly IImageRepository _imageRepository;
        private readonly IPostRepository _postRepository;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly UserManager<User> _userManager;

        public ImageControllerTests()
        {
            _photoService = A.Fake<IPhotoService>();
            _imageRepository = A.Fake<IImageRepository>();
            _postRepository = A.Fake<IPostRepository>();
            _albumRepository = A.Fake<IImageAlbumRepository>();
            _userManager = A.Fake<UserManager<User>>();

            _controller = new ImageController(_photoService, _imageRepository, _postRepository, _albumRepository, _userManager);
        }

        [Fact]
        public async Task ImageControllerTests_AddImage_ShouldReturnRedirect_WhenModelIsInvalid()
        {
            // Arrange
            var viewModel = new AddImageViewModel { ImageAlbumId = 1 };
            var tempDataProvider = A.Fake<ITempDataProvider>();
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            _controller.TempData = tempData;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.ModelState.AddModelError("Image", "Required");

            // Act
            var result = await _controller.AddImage(viewModel);

            // Assert
            _controller.TempData["Error"].Should().Be("Ошибка при загрузке: изображение не было прикреплено");

            result.Should().BeOfType<RedirectToActionResult>()
                .Which.Should().Match<RedirectToActionResult>(r =>
                    r.ActionName == "Detail" &&
                    r.ControllerName == "Album" &&
                    r.RouteValues["id"].Equals(1));
        }

        [Fact]
        public async Task ImageControllerTests_AddImage_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult<User>(null));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var viewModel = new AddImageViewModel { ImageAlbumId = 1, Image = A.Fake<IFormFile>() };

            // Act
            var result = await _controller.AddImage(viewModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ImageControllerTests_AddImage_ShouldRedirectToDetails_WhenUploadSuccessful()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(user));

            A.CallTo(() => _photoService.UploadPhotoAsync(A<IFormFile>._, A<string>._))
                .Returns(Task.FromResult((true, "test.jpg")));

            var viewModel = new AddImageViewModel { ImageAlbumId = 1, Image = A.Fake<IFormFile>() };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AddImage(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.Should().Match<RedirectToActionResult>(r =>
                    r.ActionName == "Details" &&
                    r.ControllerName == "Album" &&
                    r.RouteValues["id"].Equals(1));
        }




        [Fact]
        public async Task ImageControllerTests_DeleteImage_ShouldReturnNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _imageRepository.GetByIdAsync(A<int>._)).Returns(Task.FromResult<Image>(null));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DeleteImage(1, 1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ImageControllerTests_DeleteImage_ShouldDeleteImageAndRedirect_WhenImageExists()
        {
            // Arrange
            var image = new Image { Id = 1, ImagePath = "path/to/image.jpg" };
            A.CallTo(() => _imageRepository.GetByIdAsync(A<int>._)).Returns(Task.FromResult(image));
            A.CallTo(() => _photoService.DeletePhoto(image.ImagePath)).Returns(true);
            A.CallTo(() => _imageRepository.Delete(image)).Returns(true);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DeleteImage(1, 1);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Details");
        }

        [Fact]
        public async Task ImageControllerTests_LoadImages_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult<User>(null));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.LoadImages();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ImageControllerTests_LoadImages_ShouldReturnImagePaths_WhenUserAuthenticated()
        {
            // Arrange
            var user = new User { UserName = "testuser", Id = "user1" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(user));

            var albums = new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, UserId = user.Id },
                new ImageAlbum { Id = 2, UserId = user.Id }
            };

            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id))
                .Returns(Task.FromResult(albums));

            var images = new List<Image>
            {
                new Image { Id = 1, ImageAlbumId = 1, ImagePath = "path/to/image1.jpg" },
                new Image { Id = 2, ImageAlbumId = 2, ImagePath = "path/to/image2.jpg" },
                new Image { Id = 3, ImageAlbumId = 3, ImagePath = "path/to/image3.jpg" }
            };

            A.CallTo(() => _imageRepository.GetAllAsync())
                .Returns(Task.FromResult(images));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.LoadImages() as JsonResult;

            // Assert
            result.Should().NotBeNull();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new
            {
                success = true,
                data = new List<string> { "path/to/image1.jpg", "path/to/image2.jpg" }
            });
        }

        [Fact]
        public async Task ImageControllerTests_AddImage_ShouldNotReturnNull_WhenImageIsUploadedSuccessfully()
        {
            // Arrange
            var user = new User { UserName = "testuser", Id = "user1" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(user));

            var imageAlbumId = 1;
            var viewModel = new AddImageViewModel
            {
                ImageAlbumId = imageAlbumId,
                Image = A.Fake<IFormFile>(),
                Description = "Test image"
            };

            var imageUploadResult = (true, "testImage.jpg"); 
            A.CallTo(() => _photoService.UploadPhotoAsync(viewModel.Image, A<string>._))
                .Returns(Task.FromResult(imageUploadResult));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AddImage(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Details");
            var imagePath = $"data\\{user.UserName}\\{imageAlbumId}\\{imageUploadResult.Item2}";
            A.CallTo(() => _imageRepository.Add(A<Image>._)).MustHaveHappened();
            A.CallTo(() => _imageRepository.Add(A<Image>.That.Matches(i =>
                i.ImageAlbumId == imageAlbumId &&
                i.ImagePath == imagePath &&
                i.Description == viewModel.Description &&
                i.CreatedAt != default(DateTime)
            ))).MustHaveHappened();
        }

        [Fact]
        public async Task ImageControllerTests_AddImage_ShouldReturnNullImagePath_WhenUploadFails()
        {
            // Arrange
            var user = new User { UserName = "testuser", Id = "user1" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(Task.FromResult(user));

            var imageAlbumId = 1;
            var viewModel = new AddImageViewModel
            {
                ImageAlbumId = imageAlbumId,
                Image = A.Fake<IFormFile>(),
                Description = "Test image"
            };
            var imageUploadResult = (false, string.Empty); 
            A.CallTo(() => _photoService.UploadPhotoAsync(viewModel.Image, A<string>._))
                .Returns(Task.FromResult(imageUploadResult));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AddImage(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Details");
            A.CallTo(() => _imageRepository.Add(A<Image>._)).MustHaveHappened();
            A.CallTo(() => _imageRepository.Add(A<Image>.That.Matches(i =>
                i.ImageAlbumId == imageAlbumId &&
                i.ImagePath == null &&  
                i.Description == viewModel.Description &&
                i.CreatedAt != default(DateTime)
            ))).MustHaveHappened();
        }



    }
}
