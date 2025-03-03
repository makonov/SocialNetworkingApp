using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Controllers;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class ProjectControllerTests
    {
        private readonly ProjectController _controller;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectStatusRepository _projectStatusRepository;
        private readonly IProjectTypeRepository _projectTypeRepository;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IProjectChangeRepository _projectChangeRepository;
        private readonly IProjectAnnouncementRepository _projectAnnouncementRepository;
        private readonly IProjectFollowerRepository _followerRepository;
        private readonly IUserService _userService;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IProjectService _projectService;

        public ProjectControllerTests()
        {
            _projectRepository = A.Fake<IProjectRepository>();
            _projectStatusRepository = A.Fake<IProjectStatusRepository>();
            _projectTypeRepository = A.Fake<IProjectTypeRepository>();
            _postRepository = A.Fake<IPostRepository>();
            _likeRepository = A.Fake<ILikeRepository>();
            _projectChangeRepository = A.Fake<IProjectChangeRepository>();
            _projectAnnouncementRepository = A.Fake<IProjectAnnouncementRepository>();
            _followerRepository = A.Fake<IProjectFollowerRepository>();
            _userService = A.Fake<IUserService>();
            _albumRepository = A.Fake<IImageAlbumRepository>();
            _projectService = A.Fake<IProjectService>();

            _controller = new ProjectController(
                _projectRepository,
                _projectStatusRepository,
                _projectTypeRepository,
                _userService,
                _postRepository,
                _likeRepository,
                _projectChangeRepository,
                _projectAnnouncementRepository,
                _followerRepository,
                _albumRepository,
                _projectService
            );
        }

        [Fact]
        public async Task ProjectControllerTests_Index_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
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
        public async Task ProjectControllerTests_Index_ShouldReturnProjects_WhenUserIsAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "currentUserId", UserName = "testUser" };
            var projects = new List<Project> { new Project { Id = 1, Title = "Test Project" } };
            var projectDataList = new List<(Project, ProjectChange?, ProjectAnnouncement?)>
            {
                (new Project { Id = 1, Title = "Test Project" }, null, null)
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(currentUser));
            A.CallTo(() => _projectRepository.GetAllAsync())
                .Returns(Task.FromResult(projects));
            A.CallTo(() => _projectService.GetProjectDataList(currentUser.Id, projects))
                .Returns(Task.FromResult(projectDataList.AsEnumerable()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ProjectCatalogueViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Projects.Count().Should().Be(1);
            viewModel.Projects.First().Project.Id.Should().Be(1);
            viewModel.CurrentUserId.Should().Be(currentUser.Id);
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldReturnNotFound_WhenProjectIdIsNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(user));

            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldReturnNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(user));
            A.CallTo(() => _projectRepository.GetByIdAsync(A<int>.Ignored))
                .Returns(Task.FromResult<Project>(null));

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldReturnView_WhenProjectIsPublic()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            var project = new Project { Id = 1, IsPrivate = false };
            var posts = new List<Post> { new Post { Id = 1 } };
            var changes = new List<ProjectChange>();
            var announcements = new List<ProjectAnnouncement>();
            var members = new List<ProjectFollower>();

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(user));
            A.CallTo(() => _projectRepository.GetByIdAsync(1))
                .Returns(Task.FromResult(project));
            A.CallTo(() => _postRepository.GetAllByProjectId(1, 1, 10, 0))
                .Returns(Task.FromResult(posts));
            A.CallTo(() => _projectChangeRepository.GetByProjectIdAsync(1))
                .Returns(Task.FromResult(changes));
            A.CallTo(() => _projectAnnouncementRepository.GetByProjectIdAsync(1))
                .Returns(Task.FromResult(announcements));
            A.CallTo(() => _followerRepository.GetMembersByProjectIdAsync(1))
                .Returns(Task.FromResult(members));
            A.CallTo(() => _followerRepository.IsMember(user.Id, 1))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldReturnView_WhenProjectIsPrivateAndUserIsMember()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            var project = new Project { Id = 1, IsPrivate = true };
            var posts = new List<Post> { new Post { Id = 1 } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(user));
            A.CallTo(() => _projectRepository.GetByIdAsync(1))
                .Returns(Task.FromResult(project));
            A.CallTo(() => _followerRepository.IsMember(user.Id, 1))
                .Returns(Task.FromResult(true));
            A.CallTo(() => _postRepository.GetAllByProjectId(1, 1, 10, 0))
                .Returns(Task.FromResult(posts));

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldReturnView_WithNoPosts_WhenProjectIsPrivateAndUserIsNotMember()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            var project = new Project { Id = 1, IsPrivate = true };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(user));
            A.CallTo(() => _projectRepository.GetByIdAsync(1))
                .Returns(Task.FromResult(project));
            A.CallTo(() => _followerRepository.IsMember(user.Id, 1))
                .Returns(Task.FromResult(false));
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, 1))
                .Returns(Task.FromResult((ProjectFollower) null));

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var viewModel = viewResult.Model as ProjectViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Posts.Should().BeNull();
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldReturnPostsWithLikeStatus_WhenUserIsMember()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            var project = new Project { Id = 1, IsPrivate = false };
            var posts = new List<Post> { new Post { Id = 1 } };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(user));
            A.CallTo(() => _projectRepository.GetByIdAsync(1))
                .Returns(Task.FromResult(project));
            A.CallTo(() => _followerRepository.IsMember(user.Id, 1))
                .Returns(Task.FromResult(true));
            A.CallTo(() => _postRepository.GetAllByProjectId(1, 1, 10, 0))
                .Returns(Task.FromResult(posts));
            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, user.Id))
                .Returns(true);

            // Act
            var result = await _controller.Details(1) as ViewResult;
            var model = result?.Model as ProjectViewModel;

            // Assert
            model.Should().NotBeNull();
            model!.Posts.Should().NotBeNull();
            model.Posts.Should().ContainSingle();
            model.Posts.First().isLiked.Should().BeTrue();
        }

        [Fact]
        public async Task ProjectControllerTests_Details_ShouldSetIsFollowerToTrue_WhenUserIsAFollower()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            var project = new Project { Id = 1 };
            var follower = new ProjectFollower { UserId = "userId", ProjectId = 1 };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored))
                .Returns(Task.FromResult(user));
            A.CallTo(() => _projectRepository.GetByIdAsync(1))
                .Returns(Task.FromResult(project));
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(user.Id, 1))
                .Returns(Task.FromResult(follower));

            // Act
            var result = await _controller.Details(1) as ViewResult;
            var model = result?.Model as ProjectViewModel;

            // Assert
            model.Should().NotBeNull();
            model!.IsFollower.Should().BeTrue();
        }

        [Fact]
        public async Task ProjectControllerTests_Get_Create_ShouldReturnViewWithViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            IEnumerable<SelectListItem> statuses = new List<SelectListItem> { new SelectListItem { Text = "Active", Value = "1" } };
            IEnumerable<SelectListItem> types = new List<SelectListItem> { new SelectListItem { Text = "Technology", Value = "1" } };

            A.CallTo(() => _projectStatusRepository.GetSelectListAsync())
                .Returns(Task.FromResult(statuses));
            A.CallTo(() => _projectTypeRepository.GetSelectListAsync())
                .Returns(Task.FromResult(types));

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var viewModel = viewResult?.Model as CreateProjectViewModel;
            viewModel.Should().NotBeNull();

            viewModel.Statuses.Should().BeEquivalentTo(statuses);
            viewModel.Types.Should().BeEquivalentTo(types);
        }

        [Fact]
        public async Task ProjectControllerTests_Post_Create_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Create(new CreateProjectViewModel());

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Post_Create_ShouldReturnViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "1" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));
            _controller.ModelState.AddModelError("Title", "Title is required");
            var model = new CreateProjectViewModel
            {
                Statuses = new List<SelectListItem> { new SelectListItem { Text = "Active", Value = "1" } },
                Types = new List<SelectListItem> { new SelectListItem { Text = "Technology", Value = "1" } }
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var returnedModel = viewResult?.Model as CreateProjectViewModel;
            returnedModel.Should().BeEquivalentTo(model);
            returnedModel.Statuses.Should().NotBeNull();
            returnedModel.Types.Should().NotBeNull();
        }

        [Fact]
        public async Task ProjectControllerTests_Post_Create_ShouldRedirectToIndex_WhenModelIsValid()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var user = new User { Id = "1" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));

            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Description = "Test Description",
                Goal = "Test Goal",
                StatusId = 1,
                TypeId = 1,
                FundraisingGoal = 1000,
                FundraisingProgress = 0,
                IsPrivate = false,
                Statuses = new List<SelectListItem> { new SelectListItem { Text = "Active", Value = "1" } },
                Types = new List<SelectListItem> { new SelectListItem { Text = "Technology", Value = "1" } }
            };

            A.CallTo(() => _projectStatusRepository.GetSelectListAsync()).Returns(Task.FromResult(model.Statuses));
            A.CallTo(() => _projectTypeRepository.GetSelectListAsync()).Returns(Task.FromResult(model.Types));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");

            A.CallTo(() => _projectRepository.Add(A<Project>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _followerRepository.Add(A<ProjectFollower>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _projectChangeRepository.Add(A<ProjectChange>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _albumRepository.Add(A<ImageAlbum>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProjectControllerTests_Post_Create_ShouldRedirectToIndex_WhenModelIsValidAndFundraisingFieldsAreNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var user = new User { Id = "1" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));

            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Description = "Test Description",
                Goal = "Test Goal",
                StatusId = 1,
                TypeId = 1,
                FundraisingGoal = null,
                FundraisingProgress = null,
                IsPrivate = false,
                Statuses = new List<SelectListItem> { new SelectListItem { Text = "Active", Value = "1" } },
                Types = new List<SelectListItem> { new SelectListItem { Text = "Technology", Value = "1" } }
            };

            A.CallTo(() => _projectStatusRepository.GetSelectListAsync()).Returns(Task.FromResult(model.Statuses));
            A.CallTo(() => _projectTypeRepository.GetSelectListAsync()).Returns(Task.FromResult(model.Types));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");

            A.CallTo(() => _projectRepository.Add(A<Project>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _followerRepository.Add(A<ProjectFollower>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _projectChangeRepository.Add(A<ProjectChange>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _albumRepository.Add(A<ImageAlbum>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProjectControllerTests_Edit_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Edit(1, "Title", "Goal", "Description", 1, 1, true);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Edit_ShouldReturnForbid_WhenUserIsNotMemberOfProject()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));
            A.CallTo(() => _followerRepository.IsMember(user.Id, 1)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.Edit(1, "Title", "Goal", "Description", 1, 1, true);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Edit_ShouldReturnNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var user = new User { Id = "userId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));
            A.CallTo(() => _followerRepository.IsMember(user.Id, 1)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepository.GetByIdAsync(1)).Returns(Task.FromResult<Project>(null));

            // Act
            var result = await _controller.Edit(1, "Title", "Goal", "Description", 1, 1, true);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_Edit_ShouldRedirectToDetails_WhenProjectIsSuccessfullyUpdated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var user = new User { Id = "userId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));

            var project = new Project
            {
                Id = 1,
                Title = "Old Title",
                Goal = "Old Goal",
                Description = "Old Description",
                StatusId = 1,
                TypeId = 1,
                FundraisingGoal = 1000,
                FundraisingProgress = 200,
                IsPrivate = false
            };

            A.CallTo(() => _followerRepository.IsMember(user.Id, 1)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepository.GetByIdAsync(1)).Returns(Task.FromResult(project));

            // Act
            var result = await _controller.Edit(1, "New Title", "New Goal", "New Description", 2, 2, true, 500, 1000);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Details");
            redirectResult.RouteValues["projectId"].Should().Be(1);
            A.CallTo(() => _projectRepository.Update(A<Project>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProjectControllerTests_Edit_ShouldNullifyFundraising_WhenProjectTypeChangesFromStartupToOther()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var user = new User { Id = "userId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(user));

            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                Goal = "Test Goal",
                Description = "Test Description",
                StatusId = 1,
                TypeId = (int)ProjectTypeEnum.Startup, 
                FundraisingGoal = 1000,
                FundraisingProgress = 500,
                IsPrivate = false
            };

            A.CallTo(() => _followerRepository.IsMember(user.Id, 1)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepository.GetByIdAsync(1)).Returns(Task.FromResult(project));

            // Act
            var result = await _controller.Edit(1, "New Title", "New Goal", "New Description", 2, (int)ProjectTypeEnum.ScientificProject, true, 500, 1000);

            // Assert
            project.FundraisingGoal.Should().BeNull();
            project.FundraisingProgress.Should().BeNull();
            A.CallTo(() => _projectRepository.Update(A<Project>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ProjectControllerTests_AddChange_ShouldCreateNewChangeAndReturnPartialView()
        {
            // Arrange
            var projectId = 1;
            var description = "Change description";

            var newChange = new ProjectChange
            {
                ProjectId = projectId,
                ChangeDescription = description,
                ChangeDate = DateTime.Now
            };

            A.CallTo(() => _projectChangeRepository.Add(newChange)).Returns(true);

            // Act
            var result = _controller.AddChange(projectId, description);

            // Assert
            A.CallTo(() => _projectChangeRepository.Add(A<ProjectChange>.That.Matches(x =>
                x.ProjectId == projectId && x.ChangeDescription == description))).MustHaveHappenedOnceExactly();

            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            partialViewResult.ViewName.Should().Be("_ChangeItemPartial");
            partialViewResult.Model.Should().BeOfType<ProjectChange>();
            var model = partialViewResult.Model as ProjectChange;
            model.ProjectId.Should().Be(projectId);
            model.ChangeDescription.Should().Be(description);
        }

        [Fact]
        public void ProjectControllerTests_AddAnnouncement_ShouldCreateNewAnnouncementAndReturnPartialView()
        {
            // Arrange
            var projectId = 1;
            var description = "Announcement description";

            var newAnnouncement = new ProjectAnnouncement
            {
                ProjectId = projectId,
                Description = description,
                CreatedAt = DateTime.Now
            };

            A.CallTo(() => _projectAnnouncementRepository.Add(newAnnouncement)).Returns(true);

            // Act
            var result = _controller.AddAnnouncement(projectId, description);

            // Assert
            A.CallTo(() => _projectAnnouncementRepository.Add(A<ProjectAnnouncement>.That.Matches(x =>
                x.ProjectId == projectId && x.Description == description))).MustHaveHappenedOnceExactly();

            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            partialViewResult.ViewName.Should().Be("_AnnouncementItemPartial");
            partialViewResult.Model.Should().BeOfType<ProjectAnnouncement>();
            var model = partialViewResult.Model as ProjectAnnouncement;
            model.ProjectId.Should().Be(projectId);
            model.Description.Should().Be(description);
        }

        [Fact]
        public async Task ProjectControllerTests_DeleteChange_ShouldReturnSuccess_WhenChangeExists()
        {
            // Arrange
            var changeId = 1;
            var existingChange = new ProjectChange
            {
                Id = changeId,
                ProjectId = 1,
                ChangeDescription = "Some description",
                ChangeDate = DateTime.Now
            };

            A.CallTo(() => _projectChangeRepository.GetByIdAsync(changeId))
                .Returns(Task.FromResult(existingChange));
            A.CallTo(() => _projectChangeRepository.Delete(existingChange)).Returns(true);

            // Act
            var result = await _controller.DeleteChange(changeId);

            // Assert
            A.CallTo(() => _projectChangeRepository.Delete(A<ProjectChange>.That.Matches(c => c.Id == changeId)))
        .MustHaveHappenedOnceExactly();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = true };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_DeleteChange_ShouldReturnFailure_WhenChangeNotFound()
        {
            // Arrange
            var changeId = 1;

            A.CallTo(() => _projectChangeRepository.GetByIdAsync(changeId))
                .Returns(Task.FromResult<ProjectChange>(null));

            // Act
            var result = await _controller.DeleteChange(changeId);

            // Assert
            A.CallTo(() => _projectChangeRepository.Delete(A<ProjectChange>.Ignored)).MustNotHaveHappened();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = false, message = "Изменение не найдено." };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_EditChange_ShouldReturnSuccess_WhenChangeExists()
        {
            // Arrange
            var changeId = 1;
            var newDescription = "Updated description";
            var existingChange = new ProjectChange
            {
                Id = changeId,
                ProjectId = 1,
                ChangeDescription = "Old description",
                ChangeDate = DateTime.Now
            };

            A.CallTo(() => _projectChangeRepository.GetByIdAsync(changeId)).Returns(Task.FromResult(existingChange));
            A.CallTo(() => _projectChangeRepository.Update(existingChange)).Returns(true);

            // Act
            var result = await _controller.EditChange(changeId, newDescription);

            // Assert
            A.CallTo(() => _projectChangeRepository.Update(A<ProjectChange>.That.Matches(c => c.Id == changeId && c.ChangeDescription == newDescription)))
                .MustHaveHappenedOnceExactly();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = true, updatedDescription = newDescription };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_EditChange_ShouldReturnFailure_WhenChangeNotFound()
        {
            // Arrange
            var changeId = 1;
            var newDescription = "Updated description";
            A.CallTo(() => _projectChangeRepository.GetByIdAsync(changeId))
                .Returns(Task.FromResult<ProjectChange>(null));

            // Act
            var result = await _controller.EditChange(changeId, newDescription);

            // Assert
            A.CallTo(() => _projectChangeRepository.Update(A<ProjectChange>.Ignored)).MustNotHaveHappened();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = false, message = "Изменение не найдено." };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_DeleteAnnouncement_ShouldReturnSuccess_WhenAnnouncementExists()
        {
            // Arrange
            var announcementId = 1;
            var existingAnnouncement = new ProjectAnnouncement
            {
                Id = announcementId,
                ProjectId = 1,
                Description = "Test announcement",
                CreatedAt = DateTime.Now
            };

            A.CallTo(() => _projectAnnouncementRepository.GetByIdAsync(announcementId)).Returns(Task.FromResult(existingAnnouncement));
            A.CallTo(() => _projectAnnouncementRepository.Delete(existingAnnouncement)).Returns(true);

            // Act
            var result = await _controller.DeleteAnnouncement(announcementId);

            // Assert
            A.CallTo(() => _projectAnnouncementRepository.Delete(A<ProjectAnnouncement>.That.Matches(a => a.Id == announcementId))).MustHaveHappenedOnceExactly();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = true };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_DeleteAnnouncement_ShouldReturnFailure_WhenAnnouncementNotFound()
        {
            // Arrange
            var announcementId = 1;
            A.CallTo(() => _projectAnnouncementRepository.GetByIdAsync(announcementId)).Returns(Task.FromResult<ProjectAnnouncement>(null));

            // Act
            var result = await _controller.DeleteAnnouncement(announcementId);

            // Assert
            A.CallTo(() => _projectAnnouncementRepository.Delete(A<ProjectAnnouncement>.Ignored)).MustNotHaveHappened();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = false, message = "Изменение не найдено." };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_EditAnnouncement_ShouldReturnFailure_WhenDescriptionIsEmpty()
        {
            // Arrange
            var announcementId = 1;
            var emptyDescription = string.Empty;

            // Act
            var result = await _controller.EditAnnouncement(announcementId, emptyDescription);

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = false, message = "Заголовок и описание не могут быть пустыми." };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_EditAnnouncement_ShouldReturnSuccess_WhenAnnouncementExistsAndDescriptionIsValid()
        {
            // Arrange
            var announcementId = 1;
            var newDescription = "Updated announcement description";
            var existingAnnouncement = new ProjectAnnouncement
            {
                Id = announcementId,
                ProjectId = 1,
                Description = "Old description",
                CreatedAt = DateTime.Now
            };

            A.CallTo(() => _projectAnnouncementRepository.GetByIdAsync(announcementId))
                .Returns(Task.FromResult(existingAnnouncement));
            A.CallTo(() => _projectAnnouncementRepository.Update(existingAnnouncement)).Returns(true);

            // Act
            var result = await _controller.EditAnnouncement(announcementId, newDescription);

            // Assert
            A.CallTo(() => _projectAnnouncementRepository.Update(A<ProjectAnnouncement>.That.Matches(a => a.Description == newDescription)))
                .MustHaveHappenedOnceExactly();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = true, updatedDescription = newDescription };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_EditAnnouncement_ShouldReturnFailure_WhenAnnouncementNotFound()
        {
            // Arrange
            var announcementId = 1;
            var newDescription = "Updated description";
            A.CallTo(() => _projectAnnouncementRepository.GetByIdAsync(announcementId))
                .Returns(Task.FromResult<ProjectAnnouncement>(null));

            // Act
            var result = await _controller.EditAnnouncement(announcementId, newDescription);

            // Assert
            A.CallTo(() => _projectAnnouncementRepository.Update(A<ProjectAnnouncement>.Ignored)).MustNotHaveHappened();
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var expectedResponse = new { success = false, message = "Объявление не найдено." };
            jsonResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task ProjectControllerTests_Unsubscribe_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var projectId = 1;
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Unsubscribe(projectId);

            // Assert
            var unauthorizedResult = result as UnauthorizedResult; 
            unauthorizedResult.Should().NotBeNull();
        }

        [Fact]
        public async Task ProjectControllerTests_Unsubscribe_ShouldReturnFailure_WhenProjectNotFoundInUserSubscriptions()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var projectId = 1;
            var currentUser = new User { Id = "1", UserName = "testUser" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(currentUser.Id, projectId)).Returns(Task.FromResult<ProjectFollower>(null));

            // Act
            var result = await _controller.Unsubscribe(projectId);

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, message = "Проект не найден." });
        }

        [Fact]
        public async Task ProjectControllerTests_Unsubscribe_ShouldReturnSuccess_WhenUserUnsubscribesSuccessfully()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var projectId = 1;
            var currentUser = new User { Id = "1", UserName = "testUser" };
            var projectFollower = new ProjectFollower { UserId = currentUser.Id, ProjectId = projectId };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(currentUser.Id, projectId)).Returns(Task.FromResult(projectFollower));
            A.CallTo(() => _followerRepository.Delete(projectFollower)).Returns(true);
            A.CallTo(() => _followerRepository.GetByProjectIdAsync(projectId)).Returns(Task.FromResult(new List<ProjectFollower> { projectFollower }));

            // Act
            var result = await _controller.Unsubscribe(projectId);

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, subscriberCount = 1 });
            A.CallTo(() => _followerRepository.Delete(projectFollower)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProjectControllerTests_Subscribe_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var projectId = 1;
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Subscribe(projectId);

            // Assert
            var unauthorizedResult = result as UnauthorizedResult; 
            unauthorizedResult.Should().NotBeNull();
        }

        [Fact]
        public async Task ProjectControllerTests_Subscribe_ShouldReturnMessage_WhenUserIsAlreadySubscribed()
        {
            // Arrange
            var projectId = 1;
            var currentUser = new User { Id = "1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(currentUser.Id, projectId)).Returns(Task.FromResult(new ProjectFollower()));

            // Act
            var result = await _controller.Subscribe(projectId);

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, message = "Пользователь уже подписан на проект." });
        }

        [Fact]
        public async Task ProjectControllerTests_Subscribe_ShouldReturnSuccess_WhenSubscriptionIsSuccessful()
        {
            // Arrange
            var projectId = 1;
            var currentUser = new User { Id = "1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(currentUser.Id, projectId)).Returns(Task.FromResult<ProjectFollower>(null)); 
            A.CallTo(() => _followerRepository.GetByProjectIdAsync(projectId)).Returns(Task.FromResult(new List<ProjectFollower> { new ProjectFollower(), new ProjectFollower() })); 

            // Act
            var result = await _controller.Subscribe(projectId);

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, subscriberCount = 2 });
        }

        [Fact]
        public async Task ProjectControllerTests_AddMember_ShouldReturnNotFound_WhenProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            var studentData = "student123";
            var role = "Member";
            A.CallTo(() => _projectRepository.GetByIdAsync(projectId)).Returns(Task.FromResult<Project>(null)); 

            // Act
            var result = await _controller.AddMember(projectId, studentData, role);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.Value.Should().Be("Проект не найден.");
        }

        [Fact]
        public async Task ProjectControllerTests_AddMember_ShouldReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var projectId = 1;
            var studentData = "student123";
            var role = "Member";
            var project = new Project { Id = projectId };
            A.CallTo(() => _projectRepository.GetByIdAsync(projectId)).Returns(Task.FromResult(project)); 
            A.CallTo(() => _userService.GetUserByIdAsync(studentData)).Returns(Task.FromResult<User>(null)); 

            // Act
            var result = await _controller.AddMember(projectId, studentData, role);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.Value.Should().Be("Пользователь не найден.");
        }

        [Fact]
        public async Task ProjectControllerTests_AddMember_ShouldReturnBadRequest_WhenUserIsAlreadyMember()
        {
            // Arrange
            var projectId = 1;
            var studentData = "student123";
            var role = "Member";
            var project = new Project { Id = projectId };
            var user = new User { Id = studentData };
            var projectFollower = new ProjectFollower { ProjectId = projectId, UserId = studentData, IsMember = true };

            A.CallTo(() => _projectRepository.GetByIdAsync(projectId)).Returns(Task.FromResult(project)); 
            A.CallTo(() => _userService.GetUserByIdAsync(studentData)).Returns(Task.FromResult(user)); 
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(studentData, projectId)).Returns(Task.FromResult(projectFollower));

            // Act
            var result = await _controller.AddMember(projectId, studentData, role);

            // Assert
            var badRequestResult = result as BadRequestResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public async Task ProjectControllerTests_AddMember_ShouldReturnPartialView_WhenAddingNewMember()
        {
            // Arrange
            var projectId = 1;
            var studentData = "student123";
            var role = "Member";
            var project = new Project { Id = projectId };
            var user = new User { Id = studentData };
            var projectFollower = new ProjectFollower { ProjectId = projectId, UserId = studentData, IsMember = false };

            A.CallTo(() => _projectRepository.GetByIdAsync(projectId)).Returns(Task.FromResult(project)); 
            A.CallTo(() => _userService.GetUserByIdAsync(studentData)).Returns(Task.FromResult(user)); 
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(studentData, projectId)).Returns(Task.FromResult(projectFollower)); 

            // Act
            var result = await _controller.AddMember(projectId, studentData, role);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            partialViewResult.ViewName.Should().Be("_MemberListItemPartial");
            var projectFollowerReturned = partialViewResult.Model as ProjectFollower;
            projectFollowerReturned.Should().NotBeNull();
            projectFollowerReturned.IsMember.Should().BeTrue();
            projectFollowerReturned.Role.Should().Be(role);
        }

        [Fact]
        public async Task ProjectControllerTests_AddMember_ShouldAddNewMember_WhenProjectFollowerDoesNotExist()
        {
            // Arrange
            var projectId = 1;
            var studentData = "student123";
            var role = "Member";
            var project = new Project { Id = projectId };
            var user = new User { Id = studentData };
            ProjectFollower projectFollower = null;
            A.CallTo(() => _projectRepository.GetByIdAsync(projectId)).Returns(Task.FromResult(project)); 
            A.CallTo(() => _userService.GetUserByIdAsync(studentData)).Returns(Task.FromResult(user));
            A.CallTo(() => _followerRepository.GetByUserIdAndProjectIdAsync(studentData, projectId)).Returns(Task.FromResult(projectFollower)); 

            // Act
            var result = await _controller.AddMember(projectId, studentData, role);

            // Assert
            var partialViewResult = result as PartialViewResult;
            partialViewResult.Should().NotBeNull();
            partialViewResult.ViewName.Should().Be("_MemberListItemPartial");

            var newProjectFollower = partialViewResult.Model as ProjectFollower;
            newProjectFollower.Should().NotBeNull();
            newProjectFollower.IsMember.Should().BeTrue();
            newProjectFollower.Role.Should().Be(role);
            A.CallTo(() => _followerRepository.Add(A<ProjectFollower>.That.Matches(pf => pf.ProjectId == projectId && pf.UserId == studentData && pf.IsMember == true && pf.Role == role)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProjectControllerTests_EditMemberRole_ShouldUpdateRole_WhenMemberExists()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var memberId = 1;
            var newRole = "Admin";
            var currentUser = new User { Id = "user123" };
            var member = new ProjectFollower { Id = memberId, Role = "Member" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _followerRepository.GetByIdAsync(memberId)).Returns(Task.FromResult(member)); 

            // Act
            var result = await _controller.EditMemberRole(memberId, newRole);

            // Assert
            result.Should().BeOfType<OkResult>(); 
            member.Role.Should().Be(newRole);
            A.CallTo(() => _followerRepository.Update(A<ProjectFollower>.That.Matches(m => m.Role == newRole))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProjectControllerTests_EditMemberRole_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var memberId = 1;
            var newRole = "Admin";
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.EditMemberRole(memberId, newRole);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task ProjectControllerTests_EditMemberRole_ShouldReturnNotFound_WhenMemberDoesNotExist()
        {
            // Arrange
            var memberId = 1;
            var newRole = "Admin";
            var currentUser = new User { Id = "user123" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _followerRepository.GetByIdAsync(memberId)).Returns(Task.FromResult<ProjectFollower>(null));

            // Act
            var result = await _controller.EditMemberRole(memberId, newRole);

            // Assert
            result.Should().BeOfType<NotFoundResult>(); 
        }

        [Fact]
        public async Task ProjectControllerTests_DeleteMember_ShouldDeleteMember_WhenMemberExists()
        {
            // Arrange
            var memberId = 1;
            var member = new ProjectFollower { Id = memberId, Role = "Member" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _followerRepository.GetByIdAsync(memberId)).Returns(Task.FromResult(member)); 

            // Act
            var result = await _controller.DeleteMember(memberId);

            // Assert
            result.Should().BeOfType<OkResult>(); 
            A.CallTo(() => _followerRepository.Delete(member)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProjectControllerTests_DeleteMember_ShouldReturnNotFound_WhenMemberDoesNotExist()
        {
            // Arrange
            var memberId = 1;
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _followerRepository.GetByIdAsync(memberId)).Returns(Task.FromResult<ProjectFollower>(null)); 

            // Act
            var result = await _controller.DeleteMember(memberId);

            // Assert
            result.Should().BeOfType<NotFoundResult>(); 
        }

        [Fact]
        public async Task ProjectControllerTests_FilterProjects_ShouldReturnFilteredProjects_WhenUserIsAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var filterModel = new FindProjectViewModel
            {
                Title = "Test Project",
                TypeId = 1
            };
            var currentUser = new User { Id = "1", UserName = "Test User" };
            var filteredProjects = new List<Project> { new Project { Id = 1, Title = "Test Project", TypeId = 1 } };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _projectRepository.GetFilteredProjectsAsync(filterModel.Title, filterModel.TypeId))
                .Returns(Task.FromResult(filteredProjects)); 
            A.CallTo(() => _projectService.GetProjectDataList(
                currentUser.Id,
                filteredProjects
            )).Returns(Task.FromResult<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>>(
                new List<(Project, ProjectChange?, ProjectAnnouncement?)> { (new Project { Id = 1, Title = "Test Project" }, null, null) } ));

            // Act
            var result = await _controller.FilterProjects(filterModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull(); 
            var model = viewResult.Model as ProjectCatalogueViewModel;
            model.Should().NotBeNull(); 
            model.Projects.Should().NotBeEmpty();
            model.CurrentUserId.Should().Be(currentUser.Id); 
        }

        [Fact]
        public async Task ProjectControllerTests_FilterProjects_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var filterModel = new FindProjectViewModel
            {
                Title = "Test Project",
                TypeId = 1
            };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult((User)null));

            // Act
            var result = await _controller.FilterProjects(filterModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_MySubscriptions_ShouldReturnViewWithUserSubscriptions_WhenUserIsAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var currentUser = new User { Id = "1", UserName = "Test User" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult(new List<ProjectFollower>
            {
                new ProjectFollower { ProjectId = 1, IsMember = false, IsOwner = false },
                new ProjectFollower { ProjectId = 2, IsMember = false, IsOwner = false }
            }));

            A.CallTo(() => _projectRepository.GetByIdAsync(1)).Returns(Task.FromResult(new Project { Id = 1, Title = "Test Project 1" }));
            A.CallTo(() => _projectRepository.GetByIdAsync(2)).Returns(Task.FromResult(new Project { Id = 2, Title = "Test Project 2" }));

            A.CallTo(() => _projectService.GetProjectDataList(currentUser.Id, A<IEnumerable<Project>>._))
                .Returns(Task.FromResult<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>>(
                    new List<(Project, ProjectChange?, ProjectAnnouncement?)>
                    {
                (new Project { Id = 1, Title = "Test Project 1" }, null, null),
                (new Project { Id = 2, Title = "Test Project 2" }, null, null)
                    }
                ));

            // Act
            var result = await _controller.MySubscriptions();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull(); 
            var model = viewResult.Model as ProjectCatalogueViewModel;
            model.Should().NotBeNull(); 
            model.Projects.Should().NotBeEmpty(); 
            model.Projects.Count().Should().Be(2); 
            model.CurrentUserId.Should().Be(currentUser.Id); 
        }

        [Fact]
        public async Task ProjectControllerTests_MySubscriptions_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.MySubscriptions();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_ProjectsWithMembership_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.ProjectsWithMembership();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_ProjectsWithMembership_ShouldReturnEmptyList_WhenUserHasNoProjectsWithMembership()
        {
            // Arrange
            var currentUser = new User { Id = "user1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult(new List<ProjectFollower>())); 
            A.CallTo(() => _projectService.GetProjectDataList(currentUser.Id, A<IEnumerable<Project>>.Ignored))
                .Returns(Task.FromResult<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>>(
                    new List<(Project, ProjectChange?, ProjectAnnouncement?)>()) 
                );

            // Act
            var result = await _controller.ProjectsWithMembership();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as ProjectCatalogueViewModel;
            model.Projects.Should().BeEmpty(); 
        }


        [Fact]
        public async Task ProjectControllerTests_ProjectsWithMembership_ShouldReturnProjects()
        {
            // Arrange
            var currentUser = new User { Id = "user1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            var projectFollowers = new List<ProjectFollower>
            {
                new ProjectFollower { ProjectId = 1, IsMember = true, IsOwner = false },
                new ProjectFollower { ProjectId = 2, IsMember = true, IsOwner = false }
            };
            A.CallTo(() => _followerRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult(projectFollowers));
            A.CallTo(() => _projectRepository.GetByIdAsync(1)).Returns(Task.FromResult(new Project { Id = 1, Title = "Project 1" }));
            A.CallTo(() => _projectRepository.GetByIdAsync(2)).Returns(Task.FromResult<Project>(null)); 
            A.CallTo(() => _projectService.GetProjectDataList(currentUser.Id, A<IEnumerable<Project>>.Ignored))
                .Returns(Task.FromResult<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>>(
                new List<(Project, ProjectChange?, ProjectAnnouncement?)>
                {
                    (new Project { Id = 1, Title = "Test Project 1" }, null, null),
                    (new Project { Id = 2, Title = "Test Project 2" }, new ProjectChange(), new ProjectAnnouncement())
                }));

            // Act
            var result = await _controller.ProjectsWithMembership();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as ProjectCatalogueViewModel;
            model.Projects.Should().HaveCount(2);
        }

        [Fact]
        public async Task ProjectControllerTests_MyProjects_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.MyProjects();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task ProjectControllerTests_MyProjects_ShouldReturnEmptyList_WhenUserHasNoProjects()
        {
            // Arrange
            var currentUser = new User { Id = "user1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetAllByUserIdAsync(currentUser.Id)).Returns(Task.FromResult(new List<ProjectFollower>()));
            A.CallTo(() => _projectService.GetProjectDataList(currentUser.Id, A<IEnumerable<Project>>.Ignored))
                .Returns(Task.FromResult<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>>(
                    new List<(Project, ProjectChange?, ProjectAnnouncement?)>()));

            // Act
            var result = await _controller.MyProjects();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as ProjectCatalogueViewModel;
            model.Projects.Should().BeEmpty(); 
        }

        [Fact]
        public async Task ProjectControllerTests_MyProjects_ShouldReturnProjects_WhenUserHasProjects()
        {
            // Arrange
            var currentUser = new User { Id = "user1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _followerRepository.GetAllByUserIdAsync(currentUser.Id))
                .Returns(Task.FromResult(new List<ProjectFollower>
                {
                    new ProjectFollower { ProjectId = 1, IsOwner = true },
                    new ProjectFollower { ProjectId = 2, IsOwner = true }
                }));
            A.CallTo(() => _projectRepository.GetByIdAsync(1))
                .Returns(Task.FromResult(new Project { Id = 1, Title = "Project 1" }));
            A.CallTo(() => _projectRepository.GetByIdAsync(2))
                .Returns(Task.FromResult(new Project { Id = 2, Title = "Project 2" }));
            A.CallTo(() => _projectService.GetProjectDataList(currentUser.Id, A<IEnumerable<Project>>.Ignored))
                .Returns(Task.FromResult<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>>(
                    new List<(Project, ProjectChange?, ProjectAnnouncement?)>
                    {
                        (new Project { Id = 1, Title = "Project 1" }, null, null),
                        (new Project { Id = 2, Title = "Project 2" }, new ProjectChange(), new ProjectAnnouncement())
                    }));

            // Act
            var result = await _controller.MyProjects();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as ProjectCatalogueViewModel;
            model.Projects.Should().HaveCount(2); 
        }

        [Fact]
        public async Task ProjectControllerTests_AnnouncementsBoard_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.AnnouncementsBoard();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>(); 
        }

        [Fact]
        public async Task ProjectControllerTests_AnnouncementsBoard_ShouldReturnEmptyList_WhenNoAnnouncements()
        {
            // Arrange
            var currentUser = new User { Id = "user1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _projectAnnouncementRepository.GetAllAsync()).Returns(Task.FromResult(new List<ProjectAnnouncement>()));
            IEnumerable<SelectListItem> types = new List<SelectListItem>();
            A.CallTo(() => _projectTypeRepository.GetSelectListAsync()).Returns(Task.FromResult(types));

            // Act
            var result = await _controller.AnnouncementsBoard();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as AnnouncementsBoardViewModel;
            model.Announcements.Should().BeEmpty(); 
        }

        [Fact]
        public async Task ProjectControllerTests_FilterAnnouncemenets_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.FilterAnnouncemenets(new AnnouncementsBoardViewModel());

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ProjectControllerTests_FilterAnnouncemenets_ShouldReturnEmptyList_WhenNoAnnouncementsMatchFilter()
        {
            // Arrange
            var currentUser = new User { Id = "user1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _projectAnnouncementRepository.GetFilteredAnnouncementsAsync(A<string>.Ignored, A<int?>.Ignored))
                .Returns(Task.FromResult(new List<ProjectAnnouncement>()));
            IEnumerable<SelectListItem> types = new List<SelectListItem>();
            A.CallTo(() => _projectTypeRepository.GetSelectListAsync()).Returns(Task.FromResult(types));

            var filterModel = new AnnouncementsBoardViewModel { KeyExpression = "test", TypeId = 1 };

            // Act
            var result = await _controller.FilterAnnouncemenets(filterModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as AnnouncementsBoardViewModel;
            model.Announcements.Should().BeEmpty(); 
        }

        [Fact]
        public async Task ProjectControllerTests_FilterAnnouncemenets_ShouldReturnFilteredAnnouncements_WhenFilterMatches()
        {
            // Arrange
            var currentUser = new User { Id = "user1" };
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>.Ignored)).Returns(Task.FromResult(currentUser));
            var announcements = new List<ProjectAnnouncement>
            {
                new ProjectAnnouncement { Id = 1, Description = "Description 1" },
                new ProjectAnnouncement { Id = 2, Description = "Description 2" }
            };
            A.CallTo(() => _projectAnnouncementRepository.GetFilteredAnnouncementsAsync("test", 1)).Returns(Task.FromResult(announcements));
            IEnumerable<SelectListItem> projectTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Type 1" },
                new SelectListItem { Value = "2", Text = "Type 2" }
            };
            A.CallTo(() => _projectTypeRepository.GetSelectListAsync()).Returns(Task.FromResult(projectTypes));

            var filterModel = new AnnouncementsBoardViewModel { KeyExpression = "test", TypeId = 1 };

            // Act
            var result = await _controller.FilterAnnouncemenets(filterModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as AnnouncementsBoardViewModel;
            model.Announcements.Should().HaveCount(2); 
            model.Types.Should().HaveCount(2); 
        }

    }
}
