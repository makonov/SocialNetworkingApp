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

        public async Task<ProjectAnnouncement> GetByIdAsync(int id)
        {
            return await _context.ProjectAnnouncements.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ProjectAnnouncement>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectAnnouncements.Where(a => a.ProjectId == projectId).ToListAsync();
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
