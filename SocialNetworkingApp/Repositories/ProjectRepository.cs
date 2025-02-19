using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Project project)
        {
            _context.Add(project);
            return Save();
        }

        public bool Delete(Project project)
        {
            _context.Remove(project);
            return Save();
        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _context.Projects.Include(p => p.ProjectStatus).Include(p => p.Type).ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.Include(p => p.ProjectStatus).Include(p => p.Type).FirstOrDefaultAsync(p => p.Id == id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(Project project)
        {
            _context.Update(project);
            return Save();
        }

        public async Task<List<Project>> GetFilteredProjectsAsync(string title, int? typeId)
        {
            var query = _context.Projects.Include(p => p.ProjectStatus).Include(p => p.Type).AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(p => p.Title.Contains(title));
            }

            if (typeId.HasValue && typeId > 0)
            {
                query = query.Where(p => p.TypeId == typeId.Value);
            }

            return await query.ToListAsync();
        }
    }
}
