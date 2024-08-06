using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetAllByUserIds(string firstUserId, string secondUserId);
        Task<List<Message>> GetLastMessagesForUserAsync(string userId);
        bool Add(Message message);
        bool Update(Message message);
        bool Delete(Message message);
        bool Save();
    }
}
