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

        public async Task<Friend?> GetByUserId(string firstUserId, string secondUserId)
        {
            return await _context.Friends.FirstOrDefaultAsync(f => f.FirstUserId == firstUserId && f.SecondUserId == secondUserId);
        }

        public async Task<List<Friend>> GetByUserId(string userId)
        {
            return await _context.Friends.Include(f => f.SecondUser).Where(f => f.FirstUserId == userId).ToListAsync();
        }

        public async Task<List<string?>> GetAllIdsByUserAsync(string userId)
        {
            return await _context.Friends.Where(f => f.FirstUserId == userId).Select(f => f.SecondUserId).ToListAsync();
        }

        public async Task<List<Friend>> GetByUserId(string userId, int page, int pageSize, int lastFriendId = 0)
        {
            var query = _context.Friends
               .Include(f => f.SecondUser)
               .OrderBy(f => f.SecondUser.FirstName + " " + f.SecondUser.LastName)
               .Where(f => f.FirstUserId == userId);

            int friendsToSkip = (page - 1) * pageSize;
            query = query.Skip(friendsToSkip);

            if (lastFriendId > 0)
            {
                query = query.Where(f => f.Id < lastFriendId);
            }

            var friends = await query.Take(pageSize).ToListAsync();

            return friends;
        }

        public bool IsFriend(string firstUserId, string secondUserId)
        {
            return _context.Friends.Any(f => f.FirstUserId == firstUserId && f.SecondUserId == secondUserId);
        }
    }
}
