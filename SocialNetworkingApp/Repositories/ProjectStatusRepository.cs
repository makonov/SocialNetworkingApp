using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ProjectStatusRepository : IProjectStatusRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectStatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(ProjectStatus status)
        {
            _context.Add(status);
            return Save();
        }

        public bool Delete(ProjectStatus status)
        {
            _context.Remove(status);
            return Save();
        }

        public async Task<ProjectStatus?> GetByIdAsync(int id)
        {
            return await _context.ProjectStatuses.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync()
        {
            return await _context.ProjectStatuses
                .Select(ps => new SelectListItem
                {
                    Value = ps.Id.ToString(),
                    Text = ps.Status
                }).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(ProjectStatus status)
        {
            _context.Update(status);
            return Save();
        }
    }
}
