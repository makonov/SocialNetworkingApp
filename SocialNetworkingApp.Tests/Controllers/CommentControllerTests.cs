using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Controllers;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Security.Claims;
using SocialNetworkingApp.Repositories;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class CommentControllerTests
    {
        private readonly CommentController _controller;
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IPhotoService _photoService;
        private readonly IImageRepository _imageRepository;
        private readonly IUserService _userService;
        private readonly IImageAlbumRepository _imageAlbumRepository;

        public CommentControllerTests()
        {
            _commentRepository = A.Fake<ICommentRepository>();
            _userManager = A.Fake<UserManager<User>>();
            _albumRepository = A.Fake<IImageAlbumRepository>();
            _photoService = A.Fake<IPhotoService>();
            _imageRepository = A.Fake<IImageRepository>();
            _userService = A.Fake<IUserService>();
            _imageAlbumRepository = A.Fake<IImageAlbumRepository>();

            _controller = new CommentController(_commentRepository, _userManager, _albumRepository, _photoService, _imageRepository, _userService);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), A.Fake<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [Fact]
        public async Task CommentControllerTests_CreateComment_ShouldReturnRedirect_WhenModelIsInvalid()
        {
            // Arrange
            var viewModel = new CreateCommentViewModel { PostId = 1 };
            _controller.ModelState.AddModelError("Text", "Required");

            // Act
            var result = await _controller.CreateComment(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _controller.TempData["Error"].Should().Be("Чтобы создать комментарий, введите текст или закрепите изображение.");
        }

        [Fact]
        public async Task CommentControllerTests_CreateComment_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var viewModel = new CreateCommentViewModel { PostId = 1, Text = "Test Comment" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            var httpContext = new DefaultHttpContext();
            httpContext.User = null; 
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.CreateComment(viewModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }


        [Fact]
        public async Task CommentControllerTests_DeleteComment_ShouldReturnJsonSuccess_WhenCommentExists()
        {
            // Arrange
            var comment = new Comment { Id = 1 };
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); 
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task CommentControllerTests_DeleteComment_ShouldReturnJsonError_WhenCommentNotFound()
        {
            // Arrange
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult<Comment>(null));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); 
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, error = "Комментарий не найден" });
        }

        [Fact]
        public async Task CommentControllerTests_CreateComment_ShouldCreateCommentWithoutImage()
        {
            // Arrange
            var user = new User { Id = "1", UserName = "testuser" };
            var viewModel = new CreateCommentViewModel { PostId = 1, Text = "Test Comment" };

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                }, "mock"))
                }
            };

            // Act
            var result = await _controller.CreateComment(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            A.CallTo(() => _commentRepository.Add(A<Comment>.That.Matches(c => c.Text == "Test Comment" && c.UserId == user.Id))).MustHaveHappened();
        }

        [Fact]
        public async Task CommentControllerTests_CreateComment_ShouldCreateCommentWithUploadedImage()
        {
            // Arrange
            var user = new User { Id = "1", UserName = "testuser" };
            var viewModel = new CreateCommentViewModel { PostId = 1, Text = "Test Comment", Image = A.Fake<IFormFile>() };
            var album = new ImageAlbum { Id = 1, Name = "Изображения на стене" };

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum> { album }));

            // Исправленный мок UploadPhotoAsync под твой интерфейс
            A.CallTo(() => _photoService.UploadPhotoAsync(viewModel.Image, A<string>._))
                .Returns(Task.FromResult((true, "image.jpg")));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock"))
                }
            };

            // Act
            var result = await _controller.CreateComment(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            A.CallTo(() => _commentRepository.Add(A<Comment>.That.Matches(c => c.Text == "Test Comment" && c.UserId == user.Id && c.Image != null))).MustHaveHappened();
            A.CallTo(() => _imageRepository.Add(A<Image>.That.Matches(img => img.ImagePath.Contains("image.jpg")))).MustHaveHappened();
        }


        [Fact]
        public async Task CommentControllerTests_CreateComment_ShouldCreateCommentWithExistingImagePath()
        {
            // Arrange
            var user = new User { Id = "1", UserName = "testuser" };
            var viewModel = new CreateCommentViewModel { PostId = 1, Text = "Test Comment", ImagePath = "path/to/existing/image.jpg" };
            var existingImage = new Image { ImagePath = "path/to/existing/image.jpg" };

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(user));
            A.CallTo(() => _imageRepository.GetByPathAsync(viewModel.ImagePath)).Returns(Task.FromResult(existingImage));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                }, "mock"))
                }
            };

            // Act
            var result = await _controller.CreateComment(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            A.CallTo(() => _commentRepository.Add(A<Comment>.That.Matches(c => c.Text == "Test Comment" && c.UserId == user.Id && c.Image == existingImage))).MustHaveHappened();
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldReturnJsonFailure_WhenTextAndImageAreNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, null, null, null);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { succsess = false, error = "Комментарий не может быть пустым" }, options => options.ComparingByMembers<object>());
        }


        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "text", null, null);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, error = "Отказано в доступе" });
        }

        [Fact]
        public async Task EditComment_ShouldReturnJsonSuccess_WhenCommentExistsAndUserIsAuthor()
        {
            // Arrange
            var comment = new Comment { Id = 1, UserId = "userId", Text = "old text", UpdatedAt = DateTime.Now };
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment));

            var user = new User { Id = "userId" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", null, null);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, imagePath = (string)null, time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            comment.Text.Should().Be("new text");
        }


        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldReturnJsonFailure_WhenUserIsNotCommentAuthor()
        {
            // Arrange
            var comment = new Comment { Id = 1, UserId = "otherUserId", Text = "old text" };
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment));

            var user = new User { Id = "userId" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", null, null);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, error = "Отказано в доступе" });
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldUploadNewImage_WhenFileIsProvided()
        {
            // Arrange
            var comment = new Comment { Id = 1, UserId = "userId", Text = "old text", UpdatedAt = DateTime.Now };
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment));
            var user = new User { Id = "userId", UserName = "userId" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));
            var album = new ImageAlbum { Id = 1, Name = "Изображения на стене" };
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum> { album }));
            var imageFile = A.Fake<IFormFile>();
            A.CallTo(() => _photoService.ReplacePhotoAsync(imageFile, A<string>.Ignored, A<string>.Ignored)).Returns(Task.FromResult((true, "newImage.jpg")));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", null, imageFile);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, imagePath = "data\\userId\\1\\newImage.jpg", time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            comment.Text.Should().Be("new text");
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldUseExistingImage_WhenProvided()
        {
            // Arrange
            var comment = new Comment { Id = 1, UserId = "userId", Text = "old text", Image = new Image { ImagePath = "existing_image.jpg" } };
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment));
            var user = new User { Id = "userId", UserName = "userId" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));
            var existingImage = new Image { ImagePath = "existing_image.jpg" };
            A.CallTo(() => _imageRepository.GetByPathAsync("existing_image.jpg")).Returns(Task.FromResult(existingImage));
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", "existing_image.jpg", null);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, imagePath = "existing_image.jpg", time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            comment.Text.Should().Be("new text");
            comment.Image.ImagePath.Should().Be("existing_image.jpg");
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldRemoveImage_WhenNoNewImageAndExistingImageIsNull()
        {
            // Arrange
            var comment = new Comment { Id = 1, UserId = "userId", Text = "old text", UpdatedAt = DateTime.Now, Image = new Image { ImagePath = "existing_image.jpg" } };
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment));
            var user = new User { Id = "userId" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", null, null);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, imagePath = (string)null, time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            comment.Text.Should().Be("new text");
            comment.Image.Should().BeNull();
        }

        [Fact]
        public async Task CommentControllerTests_GetComments_ShouldReturnComments_WhenUserIsAuthorized()
        {
            // Arrange
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            var post = new Post { Id = 1, Text = "Test Post" };
            var comment1 = new Comment { Id = 1, Text = "Comment 1", Post = post };
            var comment2 = new Comment { Id = 2, Text = "Comment 2", Post = post };
            var comments = new List<Comment> { comment1, comment2 };
            A.CallTo(() => _commentRepository.GetByIdAsync(0)).Returns(Task.FromResult(comment1));
            A.CallTo(() => _commentRepository.GetByPostIdAsync(post.Id, 1, 10, 0)).Returns(Task.FromResult(comments));
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.GetComments(1, 0);

            // Assert
            result.Should().BeOfType<PartialViewResult>();
            var partialViewResult = result as PartialViewResult;
            var viewModel = partialViewResult?.Model as PostCommentsViewModel;
            viewModel.Comments.Should().HaveCount(2);
            viewModel.Comments.Should().Contain(comment1);
            viewModel.Comments.Should().Contain(comment2);
            viewModel.CurrentUserId.Should().Be(currentUser.Id);
            viewModel.PostId.Should().Be(post.Id);
            viewModel.Post.Should().Be(post);
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = currentUser }
            };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.EditComment(1, "new text", "existing_image.jpg", null);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CommentControllerTests_GetComments_ShouldReturnUnauthorized_WhenCurrentUserIsNull()
        {
            // Arrange
            var currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = currentUser }
            };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.GetComments(1, 1); 

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldReturnNullImagePath_WhenImageReplacementFails()
        {
            // Arrange
            var user = new User { Id = "userId", UserName = "testUser" };
            var comment = new Comment { Id = 1, UserId = "userId", Text = "old text", Image = null };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user)); 
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment));  
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Изображения на стене" }
            }));  
            A.CallTo(() => _photoService.ReplacePhotoAsync(A<IFormFile>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(Task.FromResult((false, string.Empty)));  
            var formFile = A.Fake<IFormFile>();
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", null, formFile);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, imagePath = (string?)null, time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            comment.Text.Should().Be("new text");
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldReturnNullImagePath_WhenCommentImageIsNull()
        {
            // Arrange
            var user = new User { Id = "userId", UserName = "testUser" };
            var comment = new Comment { Id = 1, UserId = "userId", Text = "old text", Image = null }; 
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user)); 
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment)); 
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Изображения на стене" }
            }));  
            A.CallTo(() => _photoService.ReplacePhotoAsync(A<IFormFile>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(Task.FromResult((false, string.Empty)));  
            var formFile = A.Fake<IFormFile>();
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", null, formFile);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, imagePath = (string?)null, time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            comment.Text.Should().Be("new text");
        }

        [Fact]
        public async Task CommentControllerTests_EditComment_ShouldReturnImagePath_WhenCommentImageIsNotNull()
        {
            // Arrange
            var user = new User { Id = "userId", UserName = "testUser" };
            var existingImage = new Image { Id = 1, ImagePath = "data\\testUser\\1\\existingImage.jpg" }; 
            var comment = new Comment { Id = 1, UserId = "userId", Text = "old text", Image = existingImage }; 
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));  
            A.CallTo(() => _commentRepository.GetByIdAsync(1)).Returns(Task.FromResult(comment)); 
            A.CallTo(() => _albumRepository.GetAllByUserAsync(user.Id)).Returns(Task.FromResult(new List<ImageAlbum>
            {
                new ImageAlbum { Id = 1, Name = "Изображения на стене" }
            })); 
            A.CallTo(() => _photoService.ReplacePhotoAsync(A<IFormFile>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(Task.FromResult((true, "newImageFile.jpg"))); 
            var formFile = A.Fake<IFormFile>();
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.EditComment(1, "new text", null, formFile);

            // Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, imagePath = "data\\testUser\\1\\newImageFile.jpg", time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            comment.Text.Should().Be("new text");
            comment.Image.ImagePath.Should().Be("data\\testUser\\1\\newImageFile.jpg"); 
        }



    }

}
