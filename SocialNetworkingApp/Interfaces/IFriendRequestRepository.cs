using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IFriendRequestRepository
    {
        Task<List<FriendRequest>> GetRequestsByReceiverId(string receiverId);
        FriendRequest? GetRequest(string firstUserId, string secondUserId);
        bool RequestExists(string firstUserId, string secondUserId);
        bool Add(FriendRequest request);
        bool Update(FriendRequest request);
        bool Delete(FriendRequest request);
        bool Save();
    }
}
