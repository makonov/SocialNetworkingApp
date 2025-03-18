using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface ILikeRepository
    {
        bool IsPostLikedByUser(int postId, string userId);
        Task<bool> ChangeLikeStatus(int postId, string userId);
        int GetNumberOfLikes(int postId);
        bool Add(Like like);
        bool Update(Like like);
        bool Delete(Like like);
        bool Save();
    }
}
