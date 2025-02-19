using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class CommunityMemberRepository : ICommunityMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(CommunityMember member)
        {
            _context.Add(member);
            return Save();
        }

        public bool Delete(CommunityMember member)
        {
            _context.Remove(member);
            return Save();
        }

        public async Task<List<CommunityMember?>> GetAllByUserIdAsync(string userId)
        {
            return await _context.CommunityMembers.Include(m => m.Community).Where(c => c.UserId == userId).ToListAsync();
        }

        public async Task<List<CommunityMember>> GetByCommunityIdAsync(int communityId)
        {
            return await _context.CommunityMembers.Where(c => c.CommunityId == communityId).ToListAsync();
        }

        public async Task<CommunityMember> GetByIdAsync(int id)
        {
            return await _context.CommunityMembers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CommunityMember?> GetByUserIdAndCommunityIdAsync(string userId, int communityId)
        {
            return await _context.CommunityMembers.FirstOrDefaultAsync(c => c.UserId == userId && c.CommunityId == communityId);
        }

        public async Task<bool> IsMember(string userId, int communityId)
        {
            return await _context.CommunityMembers.AnyAsync(m => m.UserId == userId && m.CommunityId == communityId);
        }

        public async Task<bool> IsAdmin(string userId, int communityId)
        {
            return await _context.CommunityMembers.AnyAsync(m => m.UserId == userId && m.CommunityId == communityId && m.IsAdmin);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(CommunityMember member)
        {
            _context.Update(member);
            return Save();
        }

        public async Task<List<CommunityMember>> GetAdminsByCommunityIdAsync(int communityId)
        {
            return await _context.CommunityMembers.Include(m => m.User).Where(m => m.CommunityId == communityId && m.IsAdmin).ToListAsync();
        }
    }
}
