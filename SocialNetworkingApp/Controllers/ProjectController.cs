using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;
using System.Drawing.Printing;
using System.Reflection;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
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
        private readonly IUserService _userService;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IProjectService _projectService;
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
            IImageAlbumRepository albumRepository,
            IProjectService projectService)
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
            _albumRepository = albumRepository;
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var projects = await _projectRepository.GetAllAsync();

            var viewModel = new ProjectCatalogueViewModel
            {
                Projects = await _projectService.GetProjectDataList(currentUser.Id, projects),
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

            var currentProject = await _projectRepository.GetByIdAsync((int)projectId);

            if (currentProject == null) return NotFound();

            bool isCurrentUserMember = await _followerRepository.IsMember(currentUser.Id, (int)projectId);

            var posts = !currentProject.IsPrivate || isCurrentUserMember ? await _postRepository.GetAllByProjectId((int) projectId, 1, PageSize, 0) : null;

            var postsWithLikeStatus = posts != null ? posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, currentUser.Id);
                return (p, isLikedByCurrentUser);
            }) : null;
            
            var changes = await _projectChangeRepository.GetByProjectIdAsync((int) projectId);
            var announcements = await _projectAnnouncementRepository.GetByProjectIdAsync((int)projectId);
            var members = await _followerRepository.GetMembersByProjectIdAsync((int) projectId);
            
            bool isCurrentUserOwner = await _followerRepository.IsOwner(currentUser.Id, (int)projectId);
            bool isFollower = (await _followerRepository.GetByUserIdAndProjectIdAsync(currentUser.Id, (int)projectId)) != null ? true : false;
            int FollowerCount = (await _followerRepository.GetByProjectIdAsync((int) projectId)).Count();
            var users = isCurrentUserMember ? await _userService.GetSelectListOfUsers() : null;

            var viewModel = new ProjectViewModel
            {
                User = currentUser,
                CurrentUserId = currentUser.Id,
                IsCurrentUserMember = isCurrentUserMember,
                Project = currentProject,
                Changes = changes,
                Announcements = announcements,
                Members = members,
                Posts = postsWithLikeStatus,
                IsFollower = isFollower,
                FollowersCount = FollowerCount,
                Statuses = await _projectStatusRepository.GetSelectListAsync(),
                Types = await _projectTypeRepository.GetSelectListAsync(),
                Users = users,
                IsOwner = isCurrentUserOwner
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
                UpdatedAt = DateTime.Now,
                IsPrivate = model.IsPrivate
            };

            _projectRepository.Add(project);

            var member = new ProjectFollower
            {
                ProjectId = project.Id,
                UserId = currentUser.Id,
                IsMember = true,
                IsOwner = true,
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
        public async Task<IActionResult> Edit(int projectId, string title, string goal, string description, int statusId, int typeId, bool isPrivate, decimal? fundraisingProgress = null, decimal? fundraisingGoal = null)
        {
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
            project.IsPrivate = isPrivate;

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
        public IActionResult AddChange(int projectId, string description)
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
        public IActionResult AddAnnouncement(int projectId, string description)
        {
            var newAnnouncement= new ProjectAnnouncement
            {
                ProjectId = projectId,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _projectAnnouncementRepository.Add(newAnnouncement);

            return PartialView("_AnnouncementItemPartial", newAnnouncement);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChange(int changeId)
        {
            var change = await _projectChangeRepository.GetByIdAsync(changeId);
            if (change != null)
            {
                _projectChangeRepository.Delete(change);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Изменение не найдено." });
        }

        [HttpPost]
        public async Task<IActionResult> EditChange(int changeId, string description)
        {
            var change = await _projectChangeRepository.GetByIdAsync(changeId);
            if (change != null)
            {
                change.ChangeDescription = description;
                _projectChangeRepository.Update(change);
                return Json(new { success = true, updatedDescription = description });
            }
            return Json(new { success = false, message = "Изменение не найдено." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnnouncement(int announcementId)
        {
            var announcement = await _projectAnnouncementRepository.GetByIdAsync(announcementId);
            if (announcement != null)
            {
                _projectAnnouncementRepository.Delete(announcement);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Изменение не найдено." });
        }

        [HttpPost]
        public async Task<IActionResult> EditAnnouncement(int announcementId, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return Json(new { success = false, message = "Заголовок и описание не могут быть пустыми." });
            }

            var announcement = await _projectAnnouncementRepository.GetByIdAsync(announcementId);
            if (announcement != null)
            {
                announcement.Description = description;
                _projectAnnouncementRepository.Update(announcement);

                return Json(new { success = true, updatedDescription = description });
            }
            return Json(new { success = false, message = "Объявление не найдено." });
        }

        [HttpPost]
        public async Task<IActionResult> Unsubscribe(int projectId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var projectFollower = await _followerRepository.GetByUserIdAndProjectIdAsync(currentUser.Id, projectId);
            
            if (projectFollower == null)
            {
                return Json(new { success = false, message = "Проект не найден." });
            }

            _followerRepository.Delete(projectFollower);

            return Json(new { success = true, subscriberCount = (await _followerRepository.GetByProjectIdAsync(projectId)).Count });
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(int projectId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var projectFollower = await _followerRepository.GetByUserIdAndProjectIdAsync(currentUser.Id, projectId);

            if (projectFollower != null)
            {
                return Json(new { success = false, message = "Пользователь уже подписан на проект." });
            }

            var newProjectFollower = new ProjectFollower
            {
                ProjectId = projectId,
                UserId = currentUser.Id,
            };

            _followerRepository.Add(newProjectFollower);

            return Json(new { success = true, subscriberCount = (await _followerRepository.GetByProjectIdAsync(projectId)).Count });
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int projectId, string studentData, string role)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return NotFound("Проект не найден.");

            var user = await _userService.GetUserByIdAsync(studentData);
            if (user == null) return NotFound("Пользователь не найден.");

            var projectFollower = await _followerRepository.GetByUserIdAndProjectIdAsync(studentData, projectId);
            if (projectFollower != null) 
            {
                if (projectFollower.IsMember) return BadRequest();
                projectFollower.IsMember = true;
                projectFollower.Role = role;
                _followerRepository.Update(projectFollower);
            }
            else
            {
                projectFollower = new ProjectFollower
                {
                    ProjectId = projectId,
                    UserId = studentData,
                    IsMember = true,
                    Role = role
                };

                _followerRepository.Add(projectFollower);
            }
           

            return PartialView("_MemberListItemPartial", projectFollower);
        }


        [HttpPost]
        public async Task<IActionResult> EditMemberRole(int memberId, string role)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var member = await _followerRepository.GetByIdAsync(memberId);
            if (member != null)
            {
                member.Role = role;
                _followerRepository.Update(member);
                return Ok();
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMember(int memberId)
        {
            var member = await _followerRepository.GetByIdAsync(memberId);
            if (member != null)
            {
                _followerRepository.Delete(member);
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> FilterProjects(FindProjectViewModel filterModel)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var filteredProjects = await _projectRepository.GetFilteredProjectsAsync(filterModel.Title, filterModel.TypeId);       

            var viewModel = new ProjectCatalogueViewModel
            {
                Projects = await _projectService.GetProjectDataList(currentUser.Id, filteredProjects),
                CurrentUserId = currentUser.Id,
                User = currentUser
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> MySubscriptions()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var projectIds = (await _followerRepository.GetAllByUserIdAsync(currentUser.Id)).Where(f => !f.IsMember && !f.IsOwner).Select(f => f.ProjectId);
            var projects = new List<Project>();
            foreach (var id in projectIds)
            {
                var project = await _projectRepository.GetByIdAsync(id);
                projects.Add(project);
            }

            var viewModel = new ProjectCatalogueViewModel
            {
                Projects = await _projectService.GetProjectDataList(currentUser.Id, projects),
                CurrentUserId = currentUser.Id,
                User = currentUser,
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> ProjectsWithMembership()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var projectIds = (await _followerRepository.GetAllByUserIdAsync(currentUser.Id)).Where(f => f.IsMember && !f.IsOwner).Select(f => f.ProjectId);
            var projects = new List<Project>();
            foreach (var id in projectIds)
            {
                var project = await _projectRepository.GetByIdAsync(id);
                projects.Add(project);
            }

            var viewModel = new ProjectCatalogueViewModel
            {
                Projects = await _projectService.GetProjectDataList(currentUser.Id, projects),
                CurrentUserId = currentUser.Id,
                User = currentUser
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> MyProjects()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var projectIds = (await _followerRepository.GetAllByUserIdAsync(currentUser.Id)).Where(f => f.IsOwner).Select(f => f.ProjectId);
            var projects = new List<Project>();
            foreach (var id in projectIds)
            {
                var project = await _projectRepository.GetByIdAsync(id);
                projects.Add(project);
            }

            var viewModel = new ProjectCatalogueViewModel
            {
                Projects = await _projectService.GetProjectDataList(currentUser.Id, projects),
                CurrentUserId = currentUser.Id,
                User = currentUser
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> AnnouncementsBoard()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var announcements = await _projectAnnouncementRepository.GetAllAsync();

            var viewModel = new AnnouncementsBoardViewModel
            {
                CurrentUserId = currentUser.Id,
                Announcements = announcements,
                Types = await _projectTypeRepository.GetSelectListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> FilterAnnouncemenets(AnnouncementsBoardViewModel filterModel)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var filteredAnnouncements = await _projectAnnouncementRepository.GetFilteredAnnouncementsAsync(filterModel.KeyExpression, filterModel.TypeId);

            var viewModel = new AnnouncementsBoardViewModel
            {
                CurrentUserId = currentUser.Id,
                Announcements = filteredAnnouncements,
                Types = await _projectTypeRepository.GetSelectListAsync()
            };

            return View("AnnouncementsBoard", viewModel);
        }

    }
}
