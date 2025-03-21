using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class FeedController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly UserManager<User> _userManager;
        private readonly IProjectFollowerRepository _projectFollowerRepository;
        private readonly ICommunityMemberRepository _communityMemberRepository;
        private const int pageSize = 10;

        public FeedController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            UserManager<User> userManager,
            IProjectFollowerRepository projectFollowerRepository,
            ICommunityMemberRepository communityMemberRepository)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _userManager = userManager;
            _projectFollowerRepository = projectFollowerRepository;
            _communityMemberRepository = communityMemberRepository;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var projectIds = (await _projectFollowerRepository.GetAllByUserIdAsync(user.Id)).Select(p => p.ProjectId).ToList();
            var communityIds = (await _communityMemberRepository.GetAllByUserIdAsync(user.Id)).Select(c => c.CommunityId).ToList();
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, projectIds, communityIds, page, pageSize);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                int likeCount = _likeRepository.GetNumberOfLikes(p.Id);
                return (p, isLikedByCurrentUser, likeCount);
            });

            var viewModel = new FeedViewModel
            {
                Posts = postsWithLikeStatus,
                CurrentUserId = user.Id
            };
            return View(viewModel);
        }
    }
}
