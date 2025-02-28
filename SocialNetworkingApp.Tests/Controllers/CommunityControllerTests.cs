using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class CommunityControllerTests
    {
        private readonly ICommunityRepository _mockCommunityRepository;
        private readonly ICommunityTypeRepository _mockCommunityTypeRepository;
        private readonly IPostRepository _mockPostRepository;
        private readonly ILikeRepository _mockLikeRepository;
        private readonly ICommunityMemberRepository _mockCommunityMemberRepository;
        private readonly IUserService _mockUserService;
        private readonly IImageAlbumRepository _mockAlbumRepository;
        private readonly CommunityController _controller;
        private readonly HttpContext _httpContext;

        public CommunityControllerTests()
        {
            _mockCommunityRepository = A.Fake<ICommunityRepository>();
            _mockCommunityTypeRepository = A.Fake<ICommunityTypeRepository>();
            _mockPostRepository = A.Fake<IPostRepository>();
            _mockLikeRepository = A.Fake<ILikeRepository>();
            _mockCommunityMemberRepository = A.Fake<ICommunityMemberRepository>();
            _mockUserService = A.Fake<IUserService>();
            _mockAlbumRepository = A.Fake<IImageAlbumRepository>();

            _httpContext = A.Fake<HttpContext>();
            _controller = new CommunityController(
                _mockCommunityRepository,
                _mockCommunityTypeRepository,
                _mockPostRepository,
                _mockLikeRepository,
                _mockCommunityMemberRepository,
                _mockUserService,
                _mockAlbumRepository
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async Task CommunityControllerTests_Index_ReturnsViewWithCorrectModel_WhenUserIsAuthenticated()
        {
            // Arrange
            var currentUser = new User { Id = "id" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);

            var communityList = new List<Community> { new Community { Id = 1, Title = "Community 1" } };
            A.CallTo(() => _mockCommunityRepository.GetAllAsync()).Returns(communityList);
            A.CallTo(() => _mockCommunityTypeRepository.GetSelectListAsync()).Returns(new List<SelectListItem>());
            var mockHttpContext = new DefaultHttpContext();

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<CommunityCatalogueViewModel>();
            var viewModel = (CommunityCatalogueViewModel)((ViewResult)result).Model;
            viewModel.Communities.Should().BeEquivalentTo(communityList);
            viewModel.User.Id.Should().Be(currentUser.Id);
        }



        [Fact]
        public async Task CommunityControllerTests_Index_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User?>(null));

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CommunityControllerTests_Details_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CommunityControllerTests_Details_ReturnsNotFound_WhenCommunityIdIsNull()
        {
            // Arrange
            var currentUser = new User { Id = "id" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);

            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CommunityControllerTests_Details_ReturnsViewWithCorrectModel_WhenCommunityIdIsValid()
        {
            // Arrange
            var currentUser = new User { Id = "id" };
            var communityId = 1;
            var community = new Community { Id = communityId, OwnerId = currentUser.Id };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };
            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = false;
                return (p, isLikedByCurrentUser);
            }).ToList();

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockPostRepository.GetAllByCommunityId(communityId, 1, 10, 0)).Returns(posts);
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(communityId)).Returns(community);
            A.CallTo(() => _mockCommunityMemberRepository.IsMember(currentUser.Id, communityId)).Returns(true);
            A.CallTo(() => _mockCommunityMemberRepository.GetByCommunityIdAsync(communityId)).Returns(Task.FromResult(new List<CommunityMember>()));
            A.CallTo(() => _mockCommunityMemberRepository.IsAdmin(currentUser.Id, communityId)).Returns(false);
            A.CallTo(() => _mockCommunityTypeRepository.GetSelectListAsync()).Returns(new List<SelectListItem>());

            var fakeTempDataFactory = A.Fake<ITempDataDictionaryFactory>();
            var mockAlbumRepository = A.Fake<IImageAlbumRepository>();

            // Создаем контроллер с правильными параметрами
            var controller = new CommunityController(
                _mockCommunityRepository,
                _mockCommunityTypeRepository,
                _mockPostRepository,
                _mockLikeRepository,
                _mockCommunityMemberRepository,
                _mockUserService,
                mockAlbumRepository
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Details(communityId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewModel = ((ViewResult)result).Model as CommunityViewModel;
            viewModel.Should().NotBeNull();
            viewModel.User.Id.Should().Be(currentUser.Id);
            viewModel.Community.Id.Should().Be(communityId);
            viewModel.Posts.Should().BeEquivalentTo(postsWithLikeStatus);
            viewModel.IsCurrentUserMember.Should().BeTrue();
            viewModel.IsOwner.Should().BeTrue();
            viewModel.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public async Task CommunityControllerTests_Details_ReturnsViewWithNullUsers_WhenUserIsNotOwner()
        {
            // Arrange
            var currentUser = new User { Id = "id" };
            var communityId = 1;
            var community = new Community { Id = communityId, OwnerId = "anotherId" };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };
            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = false;
                return (p, isLikedByCurrentUser);
            }).ToList();

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockPostRepository.GetAllByCommunityId(communityId, 1, 10, 0)).Returns(posts);
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(communityId)).Returns(community);
            A.CallTo(() => _mockCommunityMemberRepository.IsMember(currentUser.Id, communityId)).Returns(true);
            A.CallTo(() => _mockCommunityMemberRepository.GetByCommunityIdAsync(communityId)).Returns(Task.FromResult(new List<CommunityMember>()));
            A.CallTo(() => _mockCommunityMemberRepository.IsAdmin(currentUser.Id, communityId)).Returns(false);
            A.CallTo(() => _mockCommunityTypeRepository.GetSelectListAsync()).Returns(new List<SelectListItem>());

            var fakeTempDataFactory = A.Fake<ITempDataDictionaryFactory>();
            var mockAlbumRepository = A.Fake<IImageAlbumRepository>();

            var controller = new CommunityController(
                _mockCommunityRepository,
                _mockCommunityTypeRepository,
                _mockPostRepository,
                _mockLikeRepository,
                _mockCommunityMemberRepository,
                _mockUserService,
                mockAlbumRepository
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Details(communityId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewModel = ((ViewResult)result).Model as CommunityViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Users.Should().BeNull(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Details_ReturnsViewWithCorrectAdmins_WhenUserIsAdmin()
        {
            // Arrange
            var currentUser = new User { Id = "id" };
            var communityId = 1;
            var community = new Community { Id = communityId, OwnerId = currentUser.Id };
            var posts = new List<Post> { new Post { Id = 1, Text = "Post 1" } };
            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = false;
                return (p, isLikedByCurrentUser);
            }).ToList();

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockPostRepository.GetAllByCommunityId(communityId, 1, 10, 0)).Returns(posts);
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(communityId)).Returns(community);
            A.CallTo(() => _mockCommunityMemberRepository.IsMember(currentUser.Id, communityId)).Returns(true);
            A.CallTo(() => _mockCommunityMemberRepository.GetByCommunityIdAsync(communityId)).Returns(Task.FromResult(new List<CommunityMember>()));
            A.CallTo(() => _mockCommunityMemberRepository.IsAdmin(currentUser.Id, communityId)).Returns(true);
            A.CallTo(() => _mockCommunityTypeRepository.GetSelectListAsync()).Returns(new List<SelectListItem>());
            var admins = new List<CommunityMember> { new CommunityMember { UserId = currentUser.Id, CommunityId = communityId } }; 
            A.CallTo(() => _mockCommunityMemberRepository.GetAdminsByCommunityIdAsync(communityId)).Returns(Task.FromResult(admins));

            var fakeHttpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = fakeHttpContext
            };

            var mockAlbumRepository = A.Fake<IImageAlbumRepository>();
            var controller = new CommunityController(
                _mockCommunityRepository,
                _mockCommunityTypeRepository,
                _mockPostRepository,
                _mockLikeRepository,
                _mockCommunityMemberRepository,
                _mockUserService,
                mockAlbumRepository
            );
            controller.ControllerContext = controllerContext;

            // Act
            var result = await controller.Details(communityId);

            // Assert
            var viewModel = ((ViewResult)result).Model as CommunityViewModel;
            viewModel.Admins.Should().NotBeEmpty();  
            viewModel.Admins.Should().Contain(admins.First());  
        }

        [Fact]
        public async Task CommunityControllerTests_Create_ReturnsViewWithCorrectModel_WhenUserIsAuthenticated()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = new User { Id = "id" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockCommunityTypeRepository.GetSelectListAsync()).Returns(new List<SelectListItem>());
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<CreateCommunityViewModel>();
            var viewModel = (CreateCommunityViewModel)((ViewResult)result).Model;
            viewModel.Types.Should().BeEmpty(); 
        }


        [Fact]
        public async Task CommunityControllerTests_Create_Post_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = (User)null;
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.Create(new CreateCommunityViewModel());

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Create_Post_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockCommunityTypeRepository.GetSelectListAsync()).Returns(new List<SelectListItem>());

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            var modelInvalid = new CreateCommunityViewModel { Title = "", Description = "", TypeId = 1 };
            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Create(modelInvalid);

            // Assert
            result.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<CreateCommunityViewModel>(); 
            var viewModel = (CreateCommunityViewModel)((ViewResult)result).Model;
            viewModel.Types.Should().NotBeNull(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Create_Post_ReturnsRedirectToIndex_WhenModelStateIsValid()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockCommunityTypeRepository.GetSelectListAsync()).Returns(new List<SelectListItem>());

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            var modelValid = new CreateCommunityViewModel
            {
                Title = "Community Title",
                Description = "Community Description",
                TypeId = 1
            };

            A.CallTo(() => _mockCommunityRepository.Add(A<Community>._)).Returns(true);
            A.CallTo(() => _mockCommunityMemberRepository.Add(A<CommunityMember>._)).Returns(true);
            A.CallTo(() => _mockAlbumRepository.Add(A<ImageAlbum>._)).Returns(true);

            // Act
            var result = await _controller.Create(modelValid);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index"); 

            A.CallTo(() => _mockCommunityRepository.Add(A<Community>._)).MustHaveHappenedOnceExactly(); 
            A.CallTo(() => _mockCommunityMemberRepository.Add(A<CommunityMember>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockAlbumRepository.Add(A<ImageAlbum>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CommunityControllerTests_Edit_Post_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = (User)null; 
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.Edit(1, "Community Title", "Community Description", 1);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Edit_Post_ReturnsForbid_WhenUserIsNotAdmin()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockCommunityMemberRepository.IsAdmin(currentUser.Id, 1)).Returns(false);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.Edit(1, "Community Title", "Community Description", 1);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CommunityControllerTests_Edit_Post_ReturnsNotFound_WhenCommunityIsNotFound()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockCommunityMemberRepository.IsAdmin(currentUser.Id, 1)).Returns(true);
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(1)).Returns(Task.FromResult<Community>(null)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.Edit(1, "Community Title", "Community Description", 1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CommunityControllerTests_Edit_Post_ReturnsRedirectToDetails_WhenCommunityIsEdited()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var currentUser = new User { Id = "userId" };
            var community = new Community { Id = 1, Title = "Old Title", Description = "Old Description", TypeId = 1 };

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);
            A.CallTo(() => _mockCommunityMemberRepository.IsAdmin(currentUser.Id, 1)).Returns(true);
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(1)).Returns(Task.FromResult(community)); 
            A.CallTo(() => _mockCommunityRepository.Update(A<Community>._)).Returns(true); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.Edit(1, "New Title", "New Description", 2);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Details"); 

            A.CallTo(() => _mockCommunityRepository.Update(A<Community>._)).MustHaveHappenedOnceExactly(); 
        }

        [Fact]
        public async Task CommunityControllerTests_AddAdmin_Post_ReturnsNotFound_WhenCommunityIsNotFound()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(1)).Returns(Task.FromResult<Community>(null)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.AddAdmin(1, "studentId");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Сообщество не найдено."); 
        }

        [Fact]
        public async Task CommunityControllerTests_AddAdmin_Post_ReturnsNotFound_WhenUserIsNotFound()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var community = new Community { Id = 1, Title = "Community 1" };
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(1)).Returns(Task.FromResult(community)); 
            A.CallTo(() => _mockUserService.GetUserByIdAsync("studentId")).Returns(Task.FromResult<User>(null)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.AddAdmin(1, "studentId");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Пользователь не найден."); 
        }

        [Fact]
        public async Task CommunityControllerTests_AddAdmin_Post_UpdatesExistingMemberToAdmin_WhenUserIsAlreadyMember()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var community = new Community { Id = 1, Title = "Community 1" };
            var existingMember = new CommunityMember { UserId = "studentId", CommunityId = 1, IsAdmin = false };

            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(1)).Returns(Task.FromResult(community)); 
            A.CallTo(() => _mockUserService.GetUserByIdAsync("studentId")).Returns(Task.FromResult(new User { Id = "studentId" })); 
            A.CallTo(() => _mockCommunityMemberRepository.GetByUserIdAndCommunityIdAsync("studentId", 1)).Returns(Task.FromResult(existingMember));
            A.CallTo(() => _mockCommunityMemberRepository.Update(A<CommunityMember>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.AddAdmin(1, "studentId");

            // Assert
            result.Should().BeOfType<PartialViewResult>().Which.ViewName.Should().Be("_AdminListItemPartial"); 

            A.CallTo(() => _mockCommunityMemberRepository.Update(A<CommunityMember>.That.Matches(member => member.IsAdmin == true))).MustHaveHappenedOnceExactly(); 
        }



        [Fact]
        public async Task CommunityControllerTests_AddAdmin_Post_CreatesNewAdmin_WhenUserIsNotMember()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var community = new Community { Id = 1, Title = "Community 1" };

            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(1)).Returns(Task.FromResult(community));
            A.CallTo(() => _mockUserService.GetUserByIdAsync("studentId")).Returns(Task.FromResult(new User { Id = "studentId" }));
            A.CallTo(() => _mockCommunityMemberRepository.GetByUserIdAndCommunityIdAsync("studentId", 1)).Returns(Task.FromResult<CommunityMember>(null));
            A.CallTo(() => _mockCommunityMemberRepository.Add(A<CommunityMember>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.AddAdmin(1, "studentId");

            // Assert
            result.Should().BeOfType<PartialViewResult>().Which.ViewName.Should().Be("_AdminListItemPartial"); 
            A.CallTo(() => _mockCommunityMemberRepository.Add(A<CommunityMember>._)).MustHaveHappenedOnceExactly(); 
        }

        [Fact]
        public async Task CommunityControllerTests_DeleteAdmin_Post_RemovesAdmin_WhenAdminExists()
        {
            // Arrange
            var adminId = 1;
            var existingAdmin = new CommunityMember { Id = adminId, UserId = "adminId", IsAdmin = true };
            A.CallTo(() => _mockCommunityMemberRepository.GetByIdAsync(adminId)).Returns(Task.FromResult(existingAdmin)); 
            A.CallTo(() => _mockCommunityMemberRepository.Delete(A<CommunityMember>._)).Returns(true); 

            // Act
            var result = await _controller.DeleteAdmin(adminId);

            // Assert
            result.Should().BeOfType<OkResult>();
            A.CallTo(() => _mockCommunityMemberRepository.Delete(A<CommunityMember>._)).MustHaveHappenedOnceExactly(); 
        }

        [Fact]
        public async Task CommunityControllerTests_DeleteAdmin_Post_ReturnsNotFound_WhenAdminDoesNotExist()
        {
            // Arrange
            var adminId = 1;
            A.CallTo(() => _mockCommunityMemberRepository.GetByIdAsync(adminId)).Returns(Task.FromResult<CommunityMember>(null));

            // Act
            var result = await _controller.DeleteAdmin(adminId);

            // Assert
            result.Should().BeOfType<NotFoundResult>(); 
            A.CallTo(() => _mockCommunityMemberRepository.Delete(A<CommunityMember>._)).MustNotHaveHappened(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Unsubscribe_Post_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var communityId = 1;
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Unsubscribe(communityId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Unsubscribe_Post_ReturnsNotFound_WhenUserIsNotMember()
        {
            // Arrange
            var communityId = 1;
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _mockCommunityMemberRepository.GetByUserIdAndCommunityIdAsync(currentUser.Id, communityId)).Returns(Task.FromResult<CommunityMember>(null));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Unsubscribe(communityId);

            // Assert
            result.Should().BeOfType<NotFoundResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Unsubscribe_Post_RemovesUserFromCommunity_WhenUserIsMember()
        {
            // Arrange
            var communityId = 1;
            var currentUser = new User { Id = "userId" };
            var communityMember = new CommunityMember { UserId = currentUser.Id, CommunityId = communityId };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _mockCommunityMemberRepository.GetByUserIdAndCommunityIdAsync(currentUser.Id, communityId)).Returns(Task.FromResult(communityMember));
            A.CallTo(() => _mockCommunityMemberRepository.Delete(A<CommunityMember>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Unsubscribe(communityId);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            var redirectToActionResult = (RedirectToActionResult)result;
            redirectToActionResult.ActionName.Should().Be("Details");
            redirectToActionResult.RouteValues["communityId"].Should().Be(communityId);
            A.CallTo(() => _mockCommunityMemberRepository.Delete(A<CommunityMember>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CommunityControllerTests_Subscribe_Post_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var communityId = 1;
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Subscribe(communityId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Subscribe_Post_ReturnsBadRequest_WhenUserIsAlreadyMember()
        {
            // Arrange
            var communityId = 1;
            var currentUser = new User { Id = "userId" };
            var existingMember = new CommunityMember { UserId = currentUser.Id, CommunityId = communityId };

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _mockCommunityMemberRepository.GetByUserIdAndCommunityIdAsync(currentUser.Id, communityId)).Returns(Task.FromResult(existingMember));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Subscribe(communityId);

            // Assert
            result.Should().BeOfType<BadRequestResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_Subscribe_Post_AddsUserToCommunity_WhenUserIsNotMember()
        {
            // Arrange
            var communityId = 1;
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _mockCommunityMemberRepository.GetByUserIdAndCommunityIdAsync(currentUser.Id, communityId)).Returns(Task.FromResult<CommunityMember>(null)); 
            A.CallTo(() => _mockCommunityMemberRepository.Add(A<CommunityMember>._)).Returns(true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Subscribe(communityId);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            var redirectToActionResult = (RedirectToActionResult)result;
            redirectToActionResult.ActionName.Should().Be("Details"); 
            redirectToActionResult.RouteValues["communityId"].Should().Be(communityId);
            A.CallTo(() => _mockCommunityMemberRepository.Add(A<CommunityMember>._)).MustHaveHappenedOnceExactly(); 
        }

        [Fact]
        public async Task CommunityControllerTests_MyCommunities_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.MyCommunities();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_MyCommunities_ReturnsEmptyList_WhenUserHasNoCommunitiesAsOwner()
        {
            // Arrange
            var currentUser = new User { Id = "userId" };
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _mockCommunityMemberRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult<List<CommunityMember?>>(new List<CommunityMember?>()));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.MyCommunities();

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            var viewResult = (ViewResult)result;
            var viewModel = (CommunityCatalogueViewModel)viewResult.Model;
            viewModel.Communities.Should().BeEmpty(); 
        }

        [Fact]
        public async Task CommunityControllerTests_MyCommunities_ReturnsCommunities_WhenUserHasCommunitiesAsOwner()
        {
            // Arrange
            var currentUser = new User { Id = "userId" };
            var communityId1 = 1;
            var communityId2 = 2;
            var communities = new List<Community>
            {
                new Community { Id = communityId1, Title = "Community 1", OwnerId = "userId" },
                new Community { Id = communityId2, Title = "Community 2", OwnerId = "userId" }
            };

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            var communityMembers = new List<CommunityMember>
            {
                new CommunityMember { UserId = "userId", CommunityId = communityId1, Community = communities[0] },
                new CommunityMember { UserId = "userId", CommunityId = communityId2, Community = communities[1] }
            };

            A.CallTo(() => _mockCommunityMemberRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult<List<CommunityMember?>>(communityMembers));
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(communityId1)).Returns(Task.FromResult(communities[0]));
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(communityId2)).Returns(Task.FromResult(communities[1]));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.MyCommunities();

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            var viewResult = (ViewResult)result;
            var viewModel = (CommunityCatalogueViewModel)viewResult.Model;
            viewModel.Communities.Should().HaveCount(2); 
            viewModel.Communities.Should().Contain(communities[0]);
            viewModel.Communities.Should().Contain(communities[1]);
        }

        [Fact]
        public async Task CommunityControllerTests_CommunitiesWithMembership_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CommunitiesWithMembership();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_CommunitiesWithMembership_ReturnsEmptyList_WhenUserHasNoCommunities()
        {
            // Arrange
            var currentUser = new User { Id = "userId" };

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 

            // Мокируем, что у пользователя нет сообществ
            A.CallTo(() => _mockCommunityMemberRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult<List<CommunityMember>>(new List<CommunityMember>())); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CommunitiesWithMembership();

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            var viewResult = (ViewResult)result;
            var viewModel = (CommunityCatalogueViewModel)viewResult.Model;
            viewModel.Communities.Should().BeEmpty(); 
        }

        [Fact]
        public async Task CommunityControllerTests_CommunitiesWithMembership_ReturnsEmptyList_WhenAllCommunitiesBelongToUser()
        {
            // Arrange
            var currentUser = new User { Id = "userId" };

            var communityMembers = new List<CommunityMember>
            {
                new CommunityMember { UserId = "userId", CommunityId = 1, Community = new Community { OwnerId = "userId" } },
                new CommunityMember { UserId = "userId", CommunityId = 2, Community = new Community { OwnerId = "userId" } }
            };

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 

            A.CallTo(() => _mockCommunityMemberRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult(communityMembers)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CommunitiesWithMembership();

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            var viewResult = (ViewResult)result;
            var viewModel = (CommunityCatalogueViewModel)viewResult.Model;
            viewModel.Communities.Should().BeEmpty(); 
        }

        [Fact]
        public async Task CommunityControllerTests_CommunitiesWithMembership_ReturnsCommunities_WhenUserHasValidCommunities()
        {
            // Arrange
            var currentUser = new User { Id = "userId" };

            var communityMembers = new List<CommunityMember>
            {
                new CommunityMember { UserId = "userId", CommunityId = 1, Community = new Community { OwnerId = "otherUserId" } },
                new CommunityMember { UserId = "userId", CommunityId = 2, Community = new Community { OwnerId = "otherUserId" } }
            };

            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 

            A.CallTo(() => _mockCommunityMemberRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult(communityMembers)); 

            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(1)).Returns(Task.FromResult(new Community { Id = 1, Title = "Community 1" })); 
            A.CallTo(() => _mockCommunityRepository.GetByIdAsync(2)).Returns(Task.FromResult(new Community { Id = 2, Title = "Community 2" })); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CommunitiesWithMembership();

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            var viewResult = (ViewResult)result;
            var viewModel = (CommunityCatalogueViewModel)viewResult.Model;
            viewModel.Communities.Should().HaveCount(2); 
        }

        [Fact]
        public async Task CommunityControllerTests_FilterCommunities_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null)); 

            var filterModel = new FindCommunityViewModel(); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.FilterCommunities(filterModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task CommunityControllerTests_FilterCommunities_ReturnsViewModel()
        {
            // Arrange
            var currentUser = new User { Id = "userId" };
            var filterModel = new FindCommunityViewModel(); 
            A.CallTo(() => _mockUserService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            var communities = new List<Community>
            {
                new Community { Id = 1, Title = "Community 1" },
                new Community { Id = 2, Title = "Community 2" }
            };
            A.CallTo(() => _mockCommunityRepository.GetFilteredCommunitiesAsync("", null)).Returns(Task.FromResult(communities)); 

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.FilterCommunities(filterModel);

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            var viewResult = (ViewResult)result;
            var viewModel = (CommunityCatalogueViewModel)viewResult.Model;
        }


    }
}