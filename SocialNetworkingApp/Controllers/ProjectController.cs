using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;
using System.Drawing.Printing;

namespace SocialNetworkingApp.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectStatusRepository _projectStatusRepository;
        private readonly IProjectTypeRepository _projectTypeRepository;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IProjectChangeRepository _projectChangeRepository;
        private readonly IProjectAnnouncementRepository _projectAnnouncementRepository;
        private readonly IProjectFollowerRepository _followerRepository;
        private readonly IProjectFeedbackRepository _projectFeedbackRepository;
        private readonly IUserService _userService;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly int PageSize = 10;

        public ProjectController(IProjectRepository projectRepository,
            IProjectStatusRepository projectStatusRepository, 
            IProjectTypeRepository projectTypeRepository,
            IUserService userService,
            IPostRepository postRepository,
            ILikeRepository likeRepository,
            IProjectChangeRepository projectChangeRepository,
            IProjectAnnouncementRepository projectAnnouncementRepository,
            IProjectFollowerRepository projectFollowerRepository,
            IProjectFeedbackRepository projectFeedbackRepository,
            IImageAlbumRepository albumRepository)
        {
            _projectRepository = projectRepository;
            _projectStatusRepository = projectStatusRepository;
            _projectTypeRepository = projectTypeRepository;
            _userService = userService;
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _projectChangeRepository = projectChangeRepository;
            _projectAnnouncementRepository = projectAnnouncementRepository;
            _followerRepository = projectFollowerRepository;
            _projectFeedbackRepository = projectFeedbackRepository;
            _albumRepository = albumRepository;

        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var viewModel = new ProjectCatalogueViewModel
            {
                Projects = await _projectRepository.GetAllAsync(),
                CurrentUserId = currentUser.Id,
                User = currentUser
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? projectId = null)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            if (projectId == null) return NotFound();

            var posts = await _postRepository.GetAllByProjectId((int) projectId, 1, PageSize, 0);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, currentUser.Id);
                return (p, isLikedByCurrentUser);
            });

            var currentProject = await _projectRepository.GetByIdAsync((int) projectId);
            var changes = await _projectChangeRepository.GetByProjectIdAsync((int) projectId);
            var announcements = await _projectAnnouncementRepository.GetByProjectIdAsync((int)projectId);
            var members = await _followerRepository.GetMembersByProjectIdAsync((int) projectId);
            bool isCurrentUserMember = await _followerRepository.IsMember(currentUser.Id, (int) projectId);

            var viewModel = new ProjectViewModel
            {
                User = currentUser,
                CurrentUserId = currentUser.Id,
                IsCurrentUserMember = isCurrentUserMember,
                Project = currentProject,
                Changes = changes,
                Announcements = announcements,
                Members = members,
                Posts = postsWithLikeStatus
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateProjectViewModel
            {
                Statuses = await _projectStatusRepository.GetSelectListAsync(),
                Types = await _projectTypeRepository.GetSelectListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.Statuses = await _projectStatusRepository.GetSelectListAsync();
                model.Types = await _projectTypeRepository.GetSelectListAsync();

                return View(model);
            }

            var project = new Project
            {
                Title = model.Title,
                Description = model.Description,
                Goal = model.Goal,
                StatusId = model.StatusId,
                TypeId = model.TypeId,
                FundraisingGoal = model.FundraisingGoal != null ? model.FundraisingGoal : 0,
                FundraisingProgress = model.FundraisingProgress != null ? model.FundraisingProgress : 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _projectRepository.Add(project);

            var member = new ProjectFollower
            {
                ProjectId = project.Id,
                UserId = currentUser.Id,
                IsMember = true,
                IsAdmin = true,
                Role = "Создатель"
            };

            _followerRepository.Add(member);

            var change = new ProjectChange
            {
                ProjectId = project.Id,
                ChangeDescription = "Проект создан",
                ChangeDate = DateTime.Now,
            };

            _projectChangeRepository.Add(change);

            var postImages = new ImageAlbum
            {
                ProjectId = project.Id,
                Name = "Изображения на стене",
                IsRequired = true
            };

            _albumRepository.Add(postImages);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int projectId, string title, string goal, string description, int statusId, int typeId, decimal? fundraisingProgress = null, decimal? fundraisingGoal = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ошибка. Поле не может быть пустым.";
                RedirectToAction("Details", new { projectId = projectId });
            }

            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            bool isCurrentUserMember = await _followerRepository.IsMember(currentUser.Id, (int)projectId);

            if (!isCurrentUserMember) return Forbid();

            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project == null) return NotFound();

            project.Title = title;
            project.Goal = goal;
            project.Description = description;
            project.StatusId = statusId;
            project.FundraisingProgress = fundraisingProgress;
            project.FundraisingGoal = fundraisingGoal;

            if (project.TypeId == (int) ProjectTypeEnum.Startup && typeId != (int)ProjectTypeEnum.Startup)
            {
                project.FundraisingGoal = null;
                project.FundraisingProgress = null;
            }
            project.TypeId = typeId;

            _projectRepository.Update(project);

            return RedirectToAction("Details",new { projectId = projectId});
        }

        [HttpPost]
        public async Task<IActionResult> AddChange(int projectId, string description)
        {
            if (projectId > 0)
        {
            var newChange = new ProjectChange
            {
                ProjectId = projectId,
                ChangeDescription = description,
                ChangeDate = DateTime.Now
            };

            _projectChangeRepository.Add(newChange);

            return PartialView("_ChangeItemPartial", newChange); 
        }

        [HttpPost]
        public IActionResult AddAnnouncement(int projectId, string title, string description)
        {
            var newAnnouncement= new ProjectAnnouncement
            {
                ProjectId = projectId,
                Title = title,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _projectAnnouncementRepository.Add(newAnnouncement);

            return PartialView("_AnnouncementItemPartial", newAnnouncement);
        }
    }
}
