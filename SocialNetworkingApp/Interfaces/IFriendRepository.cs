using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IFriendRepository
    {
        Task<List<Friend>> GetByUserId(string userId, int page, int pageSize, int lastFriendId = 0);
        Task<Friend?> GetByUserId(string firstUserId, string secondUserId);
        Task<List<string?>> GetAllIdsByUserAsync(string userId);
        Task<List<Friend>> GetByUserId(string userId);
        bool IsFriend(string firstUserId, string secondUserId);
        bool Add(Friend friend);
        bool Update(Friend friend);
        bool Delete(Friend friend);
        bool Save();
    }
}
