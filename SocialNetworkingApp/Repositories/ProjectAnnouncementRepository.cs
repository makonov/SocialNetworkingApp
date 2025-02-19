using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ProjectAnnouncementRepository : IProjectAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectAnnouncementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(ProjectAnnouncement announcement)
        {
            _context.Add(announcement);
            return Save();
        }

        public bool Delete(ProjectAnnouncement announcement)
        {
            _context.Remove(announcement);
            return Save();
        }

        public Task<List<ProjectAnnouncement>> GetAllAsync()
        {
            return _context.ProjectAnnouncements.Include(a => a.Project).OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<ProjectAnnouncement> GetByIdAsync(int id)
        {
            return await _context.ProjectAnnouncements.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ProjectAnnouncement>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectAnnouncements.Where(a => a.ProjectId == projectId).ToListAsync();
        }

        public async Task<List<ProjectAnnouncement>> GetFilteredAnnouncementsAsync(string? keyExpression, int? projectTypeId)
        {
            var query = _context.ProjectAnnouncements
                .Include(a => a.Project)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyExpression))
            {
                query = query.Where(a => a.Title.Contains(keyExpression) || a.Description.Contains(keyExpression));
            }

            if (projectTypeId.HasValue)
            {
                query = query.Where(a => a.Project != null && a.Project.TypeId == projectTypeId);
            }

            return await query.ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(ProjectAnnouncement change)
        {
            _context.Update(change);
            return Save();
        }
    }
}
