using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class FriendRequestRepository : IFriendRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public FriendRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(FriendRequest request)
        {
            _context.Add(request);
            return Save();
        }

        public bool Delete(FriendRequest request)
        {
            _context.Remove(request);
            return Save();
        }

        public FriendRequest? GetRequest(string firstUserId, string secondUserId)
        {
            return _context.FriendRequests.FirstOrDefault(r => r.FromUserId == firstUserId && r.ToUserId == secondUserId || r.FromUserId == secondUserId && r.ToUserId == firstUserId);
        }

        public async Task<List<FriendRequest>> GetRequestsByReceiverId(string receiverId)
        {
            return await _context.FriendRequests
                .Include(r => r.FromUser)
                .Where(r => r.ToUserId == receiverId).ToListAsync();
        }

        public bool RequestExists(string firstUserId, string secondUserId)
        {
            return _context.FriendRequests.Any(r => r.FromUserId == firstUserId && r.ToUserId == secondUserId);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(FriendRequest request)
        {
            _context.Update(request);
            return Save();
        }
    }
}
