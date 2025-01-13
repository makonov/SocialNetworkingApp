using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ProjectTypeRepository : IProjectTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(ProjectType type)
        {
            _context.Add(type);
            return Save();
        }

        public bool Delete(ProjectType type)
        {
            _context.Remove(type);
            return Save();
        }

        public async Task<ProjectType?> GetByIdAsync(int id)
        {
            return await _context.ProjectTypes.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync()
        {
            return await _context.ProjectTypes
                .Select(pt => new SelectListItem
                {
                    Value = pt.Id.ToString(),
                    Text = pt.Type
                }).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(ProjectType type)
        {
            _context.Update(type);
            return Save();
        }
    }
}
