using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;

namespace SocialNetworkingApp.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public ProjectService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>> GetProjectDataList(string userId, IEnumerable<Project> projects)
        {
            var projectIds = projects
                                .Where(p => !p.IsPrivate || (_context.ProjectFolloweres
                                    .FirstOrDefault(f => f.UserId == userId && f.ProjectId == p.Id)?.IsOwner ?? false))
                                .Select(p => p.Id)
                                .ToList();


            var projectData = await _context.Projects
                .Where(p => projectIds.Contains(p.Id))
                .Select(p => new
                {
                    Project = p,
                    LastChange = _context.ProjectChanges
                        .Where(c => c.ProjectId == p.Id)
                        .OrderByDescending(c => c.ChangeDate)
                        .FirstOrDefault(),
                    LastAnnouncement = _context.ProjectAnnouncements
                        .Where(a => a.ProjectId == p.Id)
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return projectData.Select(d => (d.Project, d.LastChange, d.LastAnnouncement));
        }



    }
}
