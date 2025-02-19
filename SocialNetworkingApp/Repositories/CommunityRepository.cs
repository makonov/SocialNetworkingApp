using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Community community)
        {
            _context.Add(community);
            return Save();
        }

        public bool Delete(Community community)
        {
            _context.Remove(community);
            return Save();
        }

        public async Task<List<Community>> GetAllAsync()
        {
            return await _context.Communities.Include(c => c.CommunityType).ToListAsync();
        }

        public async Task<Community?> GetByIdAsync(int id)
        {
            return await _context.Communities.Include(c => c.CommunityType).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Community>> GetFilteredCommunitiesAsync(string title, int? typeId)
        {
            var query = _context.Communities.Include(c => c.CommunityType).AsQueryable();

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

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(Community community)
        {
            _context.Update(community);
            return Save();
        }
    }
}
