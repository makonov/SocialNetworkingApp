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

        public Task<List<Message>> GetAllMessagesByUserIds(string firstUserId, string secondUserId)
        {
            return _context.Messages
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .OrderByDescending(m => m.SentAt)
                .Where(m => m.FromUserId == firstUserId && m.ToUserId == secondUserId || m.FromUserId == secondUserId && m.ToUserId == firstUserId)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesByUserIds(string firstUserId, string secondUserId, int page, int pageSize, int lastMessageId = 0)
        {
            var query = _context.Messages
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .OrderByDescending(m => m.SentAt)
                .Where(m => m.FromUserId == firstUserId && m.ToUserId == secondUserId 
                || m.FromUserId == secondUserId && m.ToUserId == firstUserId);

            int messagesToSkip = (page - 1) * pageSize;
            query = query.Skip(messagesToSkip);

            if (lastMessageId > 0)
            {
                query = query.Where(m => m.Id < lastMessageId);
            }
            
            var messages = await query.Take(pageSize).ToListAsync();

            return messages;
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

        public Task<bool> HasUnreadMessages(string userId)
        {
            return _context.Messages.AnyAsync(m => m.ToUserId == userId && !m.IsRead);
        }
    }
}
