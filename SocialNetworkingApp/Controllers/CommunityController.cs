using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class CommunityController : Controller
    {
        private readonly ICommunityRepository _communityRepository;
        private readonly ICommunityTypeRepository _communityTypeRepository;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly ICommunityMemberRepository _communityMemberRepository;
        private readonly IUserService _userService;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly int PageSize = 10;

        public CommunityController(ICommunityRepository communityRepository, 
            ICommunityTypeRepository communityTypeRepository, 
            IPostRepository postRepository, 
            ILikeRepository likeRepository, 
            ICommunityMemberRepository communityMemberRepository, 
            IUserService userService, 
            IImageAlbumRepository albumRepository)
        {
            _communityRepository = communityRepository;
            _communityTypeRepository = communityTypeRepository;
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _communityMemberRepository = communityMemberRepository;
            _userService = userService;
            _albumRepository = albumRepository;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var viewModel = new CommunityCatalogueViewModel
            {
                Communities = await _communityRepository.GetAllAsync(),
                Types = await _communityTypeRepository.GetSelectListAsync(),
                CurrentUserId = currentUser.Id,
                User = currentUser
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? communityId = null)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            if (communityId == null) return NotFound();

            var posts = await _postRepository.GetAllByCommunityId((int)communityId, 1, PageSize, 0);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, currentUser.Id);
                int likeCount = _likeRepository.GetNumberOfLikes(p.Id);
                return (p, isLikedByCurrentUser, likeCount);
            });

            var currentCommunity = await _communityRepository.GetByIdAsync((int)communityId);
            bool isCurrentUserMember = await _communityMemberRepository.IsMember(currentUser.Id, (int)communityId);
            bool isCurrentUserOwner = currentCommunity.OwnerId == currentUser.Id ? true : false;
            int memberCount = (await _communityMemberRepository.GetByCommunityIdAsync((int)communityId)).Count();
            var users = isCurrentUserOwner ? await _userService.GetSelectListOfUsers() : null;
            var admins = await _communityMemberRepository.GetAdminsByCommunityIdAsync((int)communityId);

            var viewModel = new CommunityViewModel
            {
                User = currentUser,
                CurrentUserId = currentUser.Id,
                IsCurrentUserMember = isCurrentUserMember,
                Community = currentCommunity,
                Posts = postsWithLikeStatus,
                MemberCount = memberCount,
                Types = await _communityTypeRepository.GetSelectListAsync(),
                IsOwner = isCurrentUserOwner,
                IsAdmin = await _communityMemberRepository.IsAdmin(currentUser.Id, (int) communityId),
                Users = users,
                Admins = admins
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateCommunityViewModel
            {
                Types = await _communityTypeRepository.GetSelectListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCommunityViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.Types = await _communityTypeRepository.GetSelectListAsync();

                return View(model);
            }

            var community = new Community
            {
                Title = model.Title,
                Description = model.Description,
                OwnerId = currentUser.Id,
                TypeId = model.TypeId,
                CreatedAt = DateTime.Now
            };

            _communityRepository.Add(community);

            var member = new CommunityMember
            {
                CommunityId = community.Id,
                UserId = currentUser.Id,
                IsAdmin = true
            };

            _communityMemberRepository.Add(member);

            var postImages = new ImageAlbum
            {
                CommunityId = community.Id,
                Name = "Изображения на стене",
                IsRequired = true
            };

            _albumRepository.Add(postImages);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int communityId, string title, string description, int typeId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            bool isCurrentUserAdmin = await _communityMemberRepository.IsAdmin(currentUser.Id, communityId);

            if (!isCurrentUserAdmin) return Forbid();

            var community = await _communityRepository.GetByIdAsync(communityId);

            if (community == null) return NotFound();

            community.Title = title;
            community.Description = description;
            community.TypeId = typeId;

            _communityRepository.Update(community);

            return RedirectToAction("Details", new { communityId = communityId });
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(int communityId, string studentData)
        {
            var community = await _communityRepository.GetByIdAsync(communityId);
            if (community == null) return NotFound("Сообщество не найдено.");

            var user = await _userService.GetUserByIdAsync(studentData);
            if (user == null) return NotFound("Пользователь не найден.");

            var newAdmin = new CommunityMember();
            var communityMember = await _communityMemberRepository.GetByUserIdAndCommunityIdAsync(studentData, communityId);
            if (communityMember != null) 
            { 
                communityMember.IsAdmin = true;

                _communityMemberRepository.Update(communityMember);
            }
            else
            {
                newAdmin = new CommunityMember
                {
                    CommunityId = communityId,
                    UserId = studentData,
                    IsAdmin = true
                };

                _communityMemberRepository.Add(newAdmin);
            }
            return PartialView("_AdminListItemPartial", newAdmin);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdmin(int adminId)
        {
            var admin = await _communityMemberRepository.GetByIdAsync(adminId);
            if (admin != null)
            {
                _communityMemberRepository.Delete(admin);
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Unsubscribe(int communityId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var communityMember = await _communityMemberRepository.GetByUserIdAndCommunityIdAsync(currentUser.Id, communityId);

            if (communityMember == null)
            {
                return NotFound();
            }

            _communityMemberRepository.Delete(communityMember);

            return RedirectToAction("Details", new { communityId = communityId });
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(int communityId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var communityMember = await _communityMemberRepository.GetByUserIdAndCommunityIdAsync(currentUser.Id, communityId);

            if (communityMember != null)
            {
                return BadRequest();
            }

            var newCommunityMember = new CommunityMember
            {
                CommunityId = communityId,
                UserId = currentUser.Id,
            };

            _communityMemberRepository.Add(newCommunityMember);

            return RedirectToAction("Details", new { communityId = communityId });
        }

        public async Task<IActionResult> MyCommunities()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var communityIds = (await _communityMemberRepository.GetAllByUserIdAsync(currentUser.Id)).Where(f => f.Community.OwnerId == currentUser.Id).Select(f => f.CommunityId);
            var communities = new List<Community>();
            foreach (var id in communityIds)
            {
                var community = await _communityRepository.GetByIdAsync(id);
                communities.Add(community);
            }


            var viewModel = new CommunityCatalogueViewModel
            {
                Communities = communities,
                CurrentUserId = currentUser.Id,
                User = currentUser,
                Types = await _communityTypeRepository.GetSelectListAsync()
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> CommunitiesWithMembership()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var communityIds = (await _communityMemberRepository.GetAllByUserIdAsync(currentUser.Id)).Where(f => f.Community.OwnerId != currentUser.Id).Select(f => f.CommunityId);
            var communities = new List<Community>();
            foreach (var id in communityIds)
            {
                var community = await _communityRepository.GetByIdAsync(id);
                communities.Add(community);
            }

            var viewModel = new CommunityCatalogueViewModel
            {
                Communities = communities,
                CurrentUserId = currentUser.Id,
                User = currentUser,
                Types = await _communityTypeRepository.GetSelectListAsync()
            };

            return View("Index", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> FilterCommunities(FindCommunityViewModel filterModel)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var filteredCommunities = await _communityRepository.GetFilteredCommunitiesAsync(filterModel.Title, filterModel.TypeId);

            var viewModel = new CommunityCatalogueViewModel
            {
                Communities = filteredCommunities,
                CurrentUserId = currentUser.Id,
                User = currentUser,
                Types = await _communityTypeRepository.GetSelectListAsync()
            };

            return View("Index", viewModel);
        }
    }
}
