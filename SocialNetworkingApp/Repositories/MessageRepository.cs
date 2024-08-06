using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Message message)
        {
            _context.Add(message);
            return Save();
        }

        public bool Delete(Message message)
        {
            _context.Remove(message);
            return Save();
        }

        public bool Update(Message message)
        {
            _context.Update(message);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public Task<List<Message>> GetAllByUserIds(string firstUserId, string secondUserId)
        {
            return _context.Messages
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .OrderBy(m => m.SentAt)
                .Where(m => m.FromUserId == firstUserId && m.ToUserId == secondUserId || m.FromUserId == secondUserId && m.ToUserId == firstUserId)
                .ToListAsync();
        }

        public async Task<List<Message>> GetLastMessagesForUserAsync(string userId)
        {
            var lastMessages = await _context.Messages
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .Where(m => m.FromUserId == userId || m.ToUserId == userId)
                .GroupBy(m => new
                {
                    User1 = m.FromUserId.CompareTo(m.ToUserId) < 0 ? m.FromUserId : m.ToUserId,
                    User2 = m.FromUserId.CompareTo(m.ToUserId) < 0 ? m.ToUserId : m.FromUserId
                })
                .Select(g => g.OrderByDescending(m => m.SentAt).FirstOrDefault())
                .ToListAsync();

            return lastMessages;
        }

    }
}
