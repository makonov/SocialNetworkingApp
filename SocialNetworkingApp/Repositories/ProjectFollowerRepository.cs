using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ProjectFollowerRepository : IProjectFollowerRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectFollowerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(ProjectFollower follower)
        {
            _context.Add(follower);
            return Save();
        }

        public bool Delete(ProjectFollower follower)
        {
            _context.Remove(follower);
            return Save();
        }

        public async Task<List<ProjectFollower?>> GetAllByUserIdAsync(string userId)
        {
            return await _context.ProjectFolloweres.Include(f => f.Project).Where(f => f.UserId == userId).ToListAsync();
        }

        public async Task<ProjectFollower> GetByIdAsync(int id)
        {
            return await _context.ProjectFolloweres.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<ProjectFollower>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectFolloweres.Include(f => f.Project).Where(f => f.ProjectId == projectId).ToListAsync();
        }

        public async Task<ProjectFollower?> GetByUserIdAndProjectIdAsync(string userId, int projectId)
        {
            return await _context.ProjectFolloweres.Include(f => f.Project).FirstOrDefaultAsync(f => f.UserId == userId && f.ProjectId == projectId);
        }

        public async Task<List<ProjectFollower>> GetMembersByProjectIdAsync(int projectId)
        {
            return await _context.ProjectFolloweres.Include(f => f.User).Where(f => f.ProjectId == projectId && f.IsMember).ToListAsync();
        }

        public async Task<bool> IsMember(string userId, int projectId)
        {
            return await _context.ProjectFolloweres.AnyAsync(p => p.UserId == userId && p.ProjectId == projectId && p.IsMember);
        }

        public async Task<bool> IsOwner(string userId, int projectId)
        {
            return await _context.ProjectFolloweres.AnyAsync(p => p.UserId == userId && p.ProjectId == projectId && p.IsOwner);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(ProjectFollower follower)
        {
            _context.Update(follower);
            return Save();
        }
    }
}
