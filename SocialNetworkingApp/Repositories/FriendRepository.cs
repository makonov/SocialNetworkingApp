using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly ApplicationDbContext _context;

        public FriendRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Friend friend)
        {
            _context.Add(friend);
            return Save();
        }

        public bool Delete(Friend friend)
        {
            _context.Remove(friend);
            return Save();
        }

        public bool Update(Friend friend)
        {
            _context.Update(friend);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public async Task<List<Friend>> GetByUserId(string userId)
        {
            return await _context.Friends.Include(f => f.SecondUser).Where(f => f.FirstUserId == userId).ToListAsync();
        }

        public async Task<List<string?>> GetAllIdsByUserAsync(string userId)
        {
            return await _context.Friends.Where(f => f.FirstUserId == userId).Select(f => f.SecondUserId).ToListAsync();
        }
    }
}
