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
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class FriendControllerTests
    {
        private readonly FriendController _controller;
        private readonly UserManager<User> _userManager;
        private readonly IFriendRepository _friendRepository;
        private readonly IUserService _userService;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IStudentGroupRepository _studentGroupRepository;

        public FriendControllerTests()
        {
            _userManager = A.Fake<UserManager<User>>();
            _friendRepository = A.Fake<IFriendRepository>();
            _userService = A.Fake<IUserService>();
            _friendRequestRepository = A.Fake<IFriendRequestRepository>();
            _studentGroupRepository = A.Fake<IStudentGroupRepository>();

            _controller = new FriendController(
                _userManager,
                _friendRepository,
                _userService,
                _friendRequestRepository,
                _studentGroupRepository
            );
        }

        [Fact]
        public async Task FriendControllerTests_Index_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var httpContext = A.Fake<HttpContext>();
            A.CallTo(() => httpContext.User).Returns((ClaimsPrincipal)null);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_Index_ShouldReturnViewWithFriendsAndRequests()
        {
            // Arrange
            var user = new User { Id = "user123" };
            var friends = new List<Friend>
            {
                new Friend { Id = 1, FirstUserId = "user123", SecondUserId = "friend1" },
                new Friend { Id = 2, FirstUserId = "user123", SecondUserId = "friend2" }
            };

            var requests = new List<FriendRequest>
            {
                new FriendRequest { Id = 1, FromUserId = "request1", ToUserId = "user123" },
                new FriendRequest { Id = 2, FromUserId = "request2", ToUserId = "user123" }
            };

            var currentUser = new User { Id = "user123" };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _friendRepository.GetByUserId(currentUser.Id, 1, 10, 0)).Returns(Task.FromResult(friends));
            A.CallTo(() => _friendRequestRepository.GetRequestsByReceiverId(currentUser.Id)).Returns(Task.FromResult(requests));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() 
            };

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>(); 
            var viewResult = (ViewResult)result;
            var viewModel = (FriendsViewModel)viewResult.Model; 
            viewModel.Friends.Should().BeEquivalentTo(friends); 
            viewModel.Requests.Should().BeEquivalentTo(requests); 
        }


        [Fact]
        public async Task FriendControllerTests_GetFriends_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var httpContext = A.Fake<HttpContext>();
            A.CallTo(() => httpContext.User).Returns((ClaimsPrincipal)null);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.GetFriends(1, 0);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_GetFriends_ShouldReturnPartialViewWithFriends()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var friends = new List<Friend>
            {
                new Friend { Id = 1, FirstUserId = "user123", SecondUserId = "friend1" },
                new Friend { Id = 2, FirstUserId = "user123", SecondUserId = "friend2" }
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            A.CallTo(() => _friendRepository.GetByUserId(currentUser.Id, 1, 10, 0)).Returns(Task.FromResult(friends));

            // Act
            var result = await _controller.GetFriends(1, 0);

            // Assert
            var partialViewResult = result.Should().BeOfType<PartialViewResult>().Subject;
            var model = partialViewResult.Model.Should().BeOfType<FriendsViewModel>().Subject;
            model.Friends.Should().BeEquivalentTo(friends);
        }

        [Fact]
        public async Task FriendControllerTests_FindFiltered_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            var viewModel = new FindFriendViewModel();

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.FindFiltered(viewModel);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_FindFiltered_ShouldReturnViewWithEmptyUsers_WhenNoUsersFound()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(A<FindFriendViewModel>._, A<string>._, A<int>._, A<int>._)).Returns(Task.FromResult(new List<User>()));
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(new List<StudentGroup>()));

            var viewModel = new FindFriendViewModel();

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.FindFiltered(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            model.Users.Should().BeEmpty();
        }

        [Fact]
        public async Task FriendControllerTests_FindFiltered_ShouldReturnViewWithUsersAndStatuses_WhenUsersFound()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var users = new List<User>
            {
                new User { Id = "friend1", FirstName = "Friend", LastName = "One" },
                new User { Id = "friend2", FirstName = "Friend", LastName = "Two" }
            };

            var friendRequests = new List<FriendRequest>
            {
                new FriendRequest { FromUserId = "user123", ToUserId = "friend1" },
                new FriendRequest { FromUserId = "friend2", ToUserId = "user123" }
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(A<FindFriendViewModel>._, A<string>._, A<int>._, A<int>._)).Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(A<string>._, A<string>._))
                .ReturnsLazily((string fromUserId, string toUserId) =>
                {
                    return friendRequests.Any(fr => (fr.FromUserId == fromUserId && fr.ToUserId == toUserId) ||
                                                      (fr.FromUserId == toUserId && fr.ToUserId == fromUserId));
                });

            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(new List<StudentGroup>()));
            var viewModel = new FindFriendViewModel();

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.FindFiltered(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            model.Users.Should().HaveCount(2);
        }

        [Fact]
        public async Task FriendControllerTests_FindFiltered_ShouldReturnViewWithUsersAndReceiverStatus_WhenUserHasReceivedFriendRequest()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var users = new List<User>
            {
                new User { Id = "friend1", FirstName = "Friend", LastName = "One" },
                new User { Id = "friend2", FirstName = "Friend", LastName = "Two" }
            };

            var friendRequests = new List<FriendRequest>
            {
                new FriendRequest { FromUserId = "friend1", ToUserId = "user123" }, 
                new FriendRequest { FromUserId = "friend2", ToUserId = "user123" }  
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(A<FindFriendViewModel>._, A<string>._, A<int>._, A<int>._)).Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(A<string>._, currentUser.Id))
                .ReturnsLazily((string fromUserId, string toUserId) =>
                {
                    return friendRequests.Any(fr => fr.FromUserId == fromUserId && fr.ToUserId == toUserId);
                });
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(new List<StudentGroup>()));

            var viewModel = new FindFriendViewModel();

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.FindFiltered(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            model.Users.Should().HaveCount(2); 
        }

        [Fact]
        public async Task FriendControllerTests_FindFiltered_ShouldPopulateGroupsList_WhenGroupsAreFound()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var users = new List<User>
            {
                new User { Id = "friend1", FirstName = "Friend", LastName = "One" },
                new User { Id = "friend2", FirstName = "Friend", LastName = "Two" }
            };

            var friendRequests = new List<FriendRequest>
            {
                new FriendRequest { FromUserId = "friend1", ToUserId = "user123" },
                new FriendRequest { FromUserId = "friend2", ToUserId = "user123" }
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.FindUsersPagedAsync(A<FindFriendViewModel>._, A<string>._, A<int>._, A<int>._)).Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(A<string>._, currentUser.Id))
                .ReturnsLazily((string fromUserId, string toUserId) =>
                {
                    return friendRequests.Any(fr => fr.FromUserId == fromUserId && fr.ToUserId == toUserId);
                });

            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" },
                new StudentGroup { Id = 2, GroupName = "Group B" }
            };
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));

            var viewModel = new FindFriendViewModel();

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.FindFiltered(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;

            model.Groups.Should().HaveCount(2); 
        }

        [Fact]
        public async Task FriendControllerTests_Find_Get_ShouldReturnViewWithUsersAndStatuses_WhenUsersAreFound()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var users = new List<User>
            {
                new User { Id = "friend1", FirstName = "Friend", LastName = "One" },
                new User { Id = "friend2", FirstName = "Friend", LastName = "Two" }
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, A<int>._, A<int>._)).Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, A<string>._)).Returns(true); 
            A.CallTo(() => _friendRequestRepository.RequestExists(A<string>._, currentUser.Id)).Returns(true); 
            A.CallTo(() => _friendRepository.IsFriend(A<string>._, currentUser.Id)).Returns(true); 
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            model.Users.Should().HaveCount(2);
            model.Users.First().Item2.Should().Be(UserStatus.Sender); 
        }

        [Fact]
        public async Task FriendControllerTests_Find_Get_ShouldReturnUnauthorized_WhenCurrentUserIsNotFound()
        {
            // Arrange
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_Find_Get_ShouldReturnViewWithEmptyGroups_WhenNoGroupsExist()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var users = new List<User>
            {
                new User { Id = "friend1", FirstName = "Friend", LastName = "One" }
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, A<int>._, A<int>._)).Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(new List<StudentGroup>()));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            model.Groups.Should().BeEmpty();  
        }

        [Fact]
        public async Task FriendControllerTests_Find_Post_ShouldReturnRedirectToAction_WhenModelStateIsValid()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var viewModel = new FindFriendViewModel
            {
                FirstName = "",
                LastName = "Valid"
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group A" } };
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            var redirectToActionResult = result.As<RedirectToActionResult>();
            redirectToActionResult.ActionName.Should().Be("FindFiltered"); 
        }

        [Fact]
        public async Task FriendControllerTests_Find_Post_ShouldRedirectToFindFiltered_WhenModelStateIsValid()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var viewModel = new FindFriendViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male"
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group A" } };
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find(viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("FindFiltered");  
            redirectResult.RouteValues["FirstName"].Should().Be("John"); 
            redirectResult.RouteValues["LastName"].Should().Be("Doe");
        }

        [Fact]
        public async Task FriendControllerTests_Find_Post_ShouldReturnRedirectToAction_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var viewModel = new FindFriendViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                FromAge = -1  
            };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group A" } };
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find(viewModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>(); 
            var redirectToActionResult = result.As<RedirectToActionResult>();
            redirectToActionResult.ActionName.Should().Be("FindFiltered"); 
        }

        [Fact]
        public async Task FriendControllerTests_Find_Get_ShouldPopulateGroupsCorrectly()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var groups = new List<StudentGroup>
            {
                new StudentGroup { Id = 1, GroupName = "Group A" },
                new StudentGroup { Id = 2, GroupName = "Group B" }
            };

            var users = new List<User>
            {
                new User { Id = "user1", FirstName = "John", LastName = "Doe" },
                new User { Id = "user2", FirstName = "Jane", LastName = "Doe" }
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10)).Returns(Task.FromResult(users));
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find(users, 1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            model.Groups.Should().HaveCount(2);  
        }

        [Fact]
        public async Task FriendControllerTests_Find_Get_ShouldSetStatusToSender_WhenRequestExistsFromCurrentUser()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var userToCheck = new User { Id = "user1", FirstName = "John", LastName = "Doe" };

            var users = new List<User> { userToCheck };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10)).Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(currentUser.Id, userToCheck.Id)).Returns(true);  
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find(users, 1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            var userStatus = model.Users.FirstOrDefault().Item2;
            userStatus.Should().Be(UserStatus.Sender); 
        }

        [Fact]
        public async Task FriendControllerTests_Find_Get_ShouldSetStatusToReceiver_WhenRequestExistsFromOtherUser()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var userToCheck = new User { Id = "user1", FirstName = "John", LastName = "Doe" };

            var users = new List<User> { userToCheck };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10)).Returns(Task.FromResult(users));
            A.CallTo(() => _friendRequestRepository.RequestExists(userToCheck.Id, currentUser.Id)).Returns(true);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find(users, 1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            var userStatus = model.Users.FirstOrDefault().Item2;
            userStatus.Should().Be(UserStatus.Reciever); 
        }

        [Fact]
        public async Task FriendControllerTests_Find_Get_ShouldSetStatusToFriend_WhenUsersAreFriends()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var userToCheck = new User { Id = "user1", FirstName = "John", LastName = "Doe" };

            var users = new List<User> { userToCheck };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            A.CallTo(() => _userService.GetPagedUsers(currentUser.Id, 1, 10)).Returns(Task.FromResult(users));

            A.CallTo(() => _friendRequestRepository.RequestExists(A<string>._, A<string>._)).Returns(false); 
            A.CallTo(() => _friendRepository.IsFriend(userToCheck.Id, currentUser.Id)).Returns(true);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Find(users, 1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;

            var userStatus = model.Users.FirstOrDefault().Item2;
            userStatus.Should().Be(UserStatus.Friend);  
        }

        [Fact]
        public async Task FriendControllerTests_Find_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var currentUser = new User { Id = "user123" };
            var viewModel = new FindFriendViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                FromAge = 10, 
                ToAge = 30
            };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser));
            var groups = new List<StudentGroup> { new StudentGroup { Id = 1, GroupName = "Group A" } };
            A.CallTo(() => _studentGroupRepository.GetAllAsync()).Returns(Task.FromResult(groups));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ModelState.AddModelError("FromAge", "Поле 'Возраст от' должно быть в диапазоне от 13 до 100.");

            // Act
            var result = await _controller.Find(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<FindFriendViewModel>().Subject;
            viewResult.ViewData.ModelState.Should().ContainKey("FromAge");
            viewResult.ViewData.ModelState["FromAge"].Errors.Should().NotBeEmpty(); 
        }

        [Fact]
        public async Task FriendControllerTests_Find_Post_ShouldReturnUnauthorized_WhenCurrentUserIsNull()
        {
            // Arrange
            var viewModel = new FindFriendViewModel
            {
                FirstName = "John",
                LastName = "Doe"
            };
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Find(viewModel);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedResult>().Subject;
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task FriendControllerTests_SendRequest_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var userId = "user123";
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.SendRequest(userId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_SendRequest_ShouldReturnSuccess_WhenRequestIsAddedSuccessfully()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.Add(A<FriendRequest>._)).Returns(true); 
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.SendRequest(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task FriendControllerTests_SendRequest_ShouldReturnError_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.Add(A<FriendRequest>._)).Throws(new Exception("Database error"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.SendRequest(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, error = "Произошла ошибка при обработке запроса на добавление в друзья." });
        }

        [Fact]
        public async Task FriendControllerTests_DenyRequest_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var userId = "user123";
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null)); 
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DenyRequest(userId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_DenyRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.GetRequest(currentUser.Id, userId)).Returns((FriendRequest?)null); 
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DenyRequest(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task FriendControllerTests_DenyRequest_ShouldReturnSuccess_WhenRequestIsDeletedSuccessfully()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            var friendRequest = new FriendRequest { FromUserId = "currentUserId", ToUserId = userId };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.GetRequest(currentUser.Id, userId)).Returns(friendRequest); 
            A.CallTo(() => _friendRequestRepository.Delete(friendRequest)).Returns(true);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DenyRequest(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task FriendControllerTests_DenyRequest_ShouldReturnError_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            var friendRequest = new FriendRequest { FromUserId = "currentUserId", ToUserId = userId };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.GetRequest(currentUser.Id, userId)).Returns(friendRequest); 
            A.CallTo(() => _friendRequestRepository.Delete(friendRequest)).Throws(new Exception("Database error"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DenyRequest(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, error = "Произошла ошибка при обработке запроса на добавление в друзья." });
        }

        [Fact]
        public async Task FriendControllerTests_AcceptRequest_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var userId = "user123";
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null)); 
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AcceptRequest(userId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_AcceptRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.GetRequest(currentUser.Id, userId)).Returns((FriendRequest?)null);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AcceptRequest(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task FriendControllerTests_AcceptRequest_ShouldReturnNotFound_WhenFriendNotFound()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            var friendRequest = new FriendRequest { FromUserId = "currentUserId", ToUserId = userId };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.GetRequest(currentUser.Id, userId)).Returns(friendRequest);
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult<User>(null));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AcceptRequest(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task FriendControllerTests_AcceptRequest_ShouldReturnSuccess_WhenRequestIsAccepted()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            var friendRequest = new FriendRequest { FromUserId = "currentUserId", ToUserId = userId };
            var friend = new User { Id = userId, FirstName = "John", LastName = "Doe", ProfilePicture = "profilePicUrl" };

            var firstRelation = new Friend { FirstUserId = currentUser.Id, SecondUserId = userId };
            var secondRelation = new Friend { FirstUserId = userId, SecondUserId = currentUser.Id };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.GetRequest(currentUser.Id, userId)).Returns(friendRequest); 
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult(friend)); 
            A.CallTo(() => _friendRepository.Add(firstRelation)).Returns(true); 
            A.CallTo(() => _friendRepository.Add(secondRelation)).Returns(true); 
            A.CallTo(() => _friendRequestRepository.Delete(friendRequest)).Returns(true);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AcceptRequest(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new
            {
                success = true,
                friendId = friend.Id,
                FirstName = friend.FirstName,
                LastName = friend.LastName,
                ProfilePicture = friend.ProfilePicture
            });
        }

        [Fact]
        public async Task FriendControllerTests_AcceptRequest_ShouldReturnError_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            var friendRequest = new FriendRequest { FromUserId = "currentUserId", ToUserId = userId };
            var friend = new User { Id = userId, FirstName = "John", LastName = "Doe", ProfilePicture = "profilePicUrl" };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRequestRepository.GetRequest(currentUser.Id, userId)).Returns(friendRequest); 
            A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(Task.FromResult(friend)); 
            A.CallTo(() => _friendRepository.Add(A<Friend>._)).Throws(new Exception("Database error"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AcceptRequest(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, error = "Произошла ошибка при обработке запроса на добавление в друзья." });
        }

        [Fact]
        public async Task FriendControllerTests_DeleteFriend_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var userId = "user123";
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null)); 
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DeleteFriend(userId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FriendControllerTests_DeleteFriend_ShouldReturnNotFound_WhenFriendshipDoesNotExist()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRepository.GetByUserId(currentUser.Id, userId)).Returns((Friend?)null); 
            A.CallTo(() => _friendRepository.GetByUserId(userId, currentUser.Id)).Returns((Friend?)null);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DeleteFriend(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task FriendControllerTests_DeleteFriend_ShouldReturnSuccess_WhenFriendshipIsDeleted()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            var firstRelation = new Friend { FirstUserId = currentUser.Id, SecondUserId = userId };
            var secondRelation = new Friend { FirstUserId = userId, SecondUserId = currentUser.Id };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRepository.GetByUserId(currentUser.Id, userId)).Returns(firstRelation); 
            A.CallTo(() => _friendRepository.GetByUserId(userId, currentUser.Id)).Returns(secondRelation); 
            A.CallTo(() => _friendRepository.Delete(firstRelation)).Returns(true); 
            A.CallTo(() => _friendRepository.Delete(secondRelation)).Returns(true); 
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DeleteFriend(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task FriendControllerTests_DeleteFriend_ShouldReturnError_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = "user123";
            var currentUser = new User { Id = "currentUserId" };
            var firstRelation = new Friend { FirstUserId = currentUser.Id, SecondUserId = userId };
            var secondRelation = new Friend { FirstUserId = userId, SecondUserId = currentUser.Id };

            A.CallTo(() => _userService.GetCurrentUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(currentUser)); 
            A.CallTo(() => _friendRepository.GetByUserId(currentUser.Id, userId)).Returns(firstRelation); 
            A.CallTo(() => _friendRepository.GetByUserId(userId, currentUser.Id)).Returns(secondRelation); 
            A.CallTo(() => _friendRepository.Delete(firstRelation)).Throws(new Exception("Database error"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
            // Act
            var result = await _controller.DeleteFriend(userId);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, error = "Произошла ошибка при удалении друга." });
        }


    }

}
