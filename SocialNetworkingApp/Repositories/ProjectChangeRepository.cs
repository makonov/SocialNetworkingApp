using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ProjectChangeRepository : IProjectChangeRepository
    {
        private readonly ApplicationDbContext _context;
        public ProjectChangeRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public bool Add(ProjectChange change)
        {
            _context.Add(change);
            return Save();
        }

        public bool Delete(ProjectChange change)
        {
            _context.Remove(change);
            return Save();
        }

        public async Task<ProjectChange> GetByIdAsync(int id)
        {
            return await _context.ProjectChanges.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<ProjectChange>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectChanges.Where(c => c.ProjectId == projectId).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(ProjectChange change)
        {
            _context.Update(change);
            return Save();
        }
    }
}
