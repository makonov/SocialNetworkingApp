using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllBySubscription(string userId, List<string> friendIds, int page, int pageSize, int lastPostId = 0);
        Task<Post> GetByIdAsync(int id);
        bool Add(Post post);
        bool Update(Post post);
        bool Delete(Post post);
        bool Save();
    }
}
