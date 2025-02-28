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
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class FeedControllerTests
    {
        private readonly FeedController _controller;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly UserManager<User> _userManager;
        private readonly IProjectFollowerRepository _projectFollowerRepository;
        private readonly ICommunityMemberRepository _communityMemberRepository;

        public FeedControllerTests()
        {
            _postRepository = A.Fake<IPostRepository>();
            _likeRepository = A.Fake<ILikeRepository>();
            _friendRepository = A.Fake<IFriendRepository>();
            _userManager = A.Fake<UserManager<User>>();
            _projectFollowerRepository = A.Fake<IProjectFollowerRepository>();
            _communityMemberRepository = A.Fake<ICommunityMemberRepository>();

            _controller = new FeedController(
                _postRepository,
                _likeRepository,
                _friendRepository,
                _userManager,
                _projectFollowerRepository,
                _communityMemberRepository
            );
        }

        [Fact]
        public async Task FeedControllerTests_Index_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            var userPrincipal = A.Fake<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            A.CallTo(() => _userManager.GetUserAsync(userPrincipal)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task FeedControllerTests_Index_ShouldReturnView_WithValidUser()
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
            A.CallTo(() => _friendRepository.GetAllIdsByUserAsync(user.Id)).Returns(Task.FromResult(new List<string>()));
            A.CallTo(() => _projectFollowerRepository.GetAllByUserIdAsync(user.Id)).Returns(Task.FromResult(new List<ProjectFollower>()));
            A.CallTo(() => _communityMemberRepository.GetAllByUserIdAsync(user.Id)).Returns(Task.FromResult(new List<CommunityMember>()));
            A.CallTo(() => _postRepository.GetAllBySubscription(user.Id, A<List<string>>._, A<List<int>>._, A<List<int>>._, A<int>._, A<int>._, 0)).Returns(Task.FromResult(new List<Post>()));


            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task FeedControllerTests_Index_ShouldReturnPostsWithLikeStatus()
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

            var fakePosts = new List<Post>
            {
                new Post { Id = 1, Text = "Post 1" },
                new Post { Id = 2, Text = "Post 2" }
            };

            A.CallTo(() => _postRepository.GetAllBySubscription(
                user.Id, A<List<string>>._, A<List<int>>._, A<List<int>>._, A<int>._, A<int>._, A<int>._))
                .Returns(Task.FromResult(fakePosts));

            A.CallTo(() => _likeRepository.IsPostLikedByUser(1, user.Id)).Returns(true);
            A.CallTo(() => _likeRepository.IsPostLikedByUser(2, user.Id)).Returns(false);

            // Act
            var result = await _controller.Index();
            var viewResult = result as ViewResult;
            var viewModel = viewResult.Model as FeedViewModel;

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.Posts.Should().HaveCount(2);
            viewModel.Posts.Should().ContainSingle(p => p.data.Id == 1 && p.isLiked);
            viewModel.Posts.Should().ContainSingle(p => p.data.Id == 2 && !p.isLiked);
        }
    }

}
