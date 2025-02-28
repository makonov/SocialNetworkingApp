using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetAllMessagesByUserIds(string firstUserId, string secondUserId);
        Task<List<Message>> GetMessagesByUserIds(string firstUserId, string secondUserId, int page, int pageSize, int lastMessageId = 0);
        Task<List<Message>> GetLastMessagesForUserAsync(string userId);
        Task<bool> HasUnreadMessages(string userId);
        bool Add(Message message);
        bool Update(Message message);
        bool Delete(Message message);
        bool Save();
    }
}
