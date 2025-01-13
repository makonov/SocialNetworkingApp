using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ProjectFeedbackRepository : IProjectFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectFeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(ProjectFeedback feedback)
        {
            _context.Add(feedback);
            return Save();
        }

        public bool Delete(ProjectFeedback follower)
        {
            _context.Remove(follower);
            return Save();
        }

        public async Task<ProjectFeedback> GetByIdAsync(int id)
        {
            return await _context.ProjectFeedbacks.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<ProjectFeedback>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectFeedbacks.Where(f => f.ProjectId == projectId).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(ProjectFeedback follower)
        {
            _context.Update(follower);
            return Save();
        }
    }
}
