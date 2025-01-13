using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllBySubscription(string userId, List<string> friendIds, List<int> projectIds, int page, int pageSize, int lastPostId = 0);
        Task<List<Post>> GetAllByCommunityId(int communityId, int page, int pageSize, int lastPostId = 0);
        Task<List<Post>> GetAllByProjectId(int projectId, int page, int pageSize, int lastPostId = 0);
        Task<List<Post>> GetAllFromProfileByUserId(string userId, int page, int pageSize, int lastPostId = 0);
        Task<Post> GetByIdAsync(int id);
        Task<List<Post>> GetAllEmptyAsync();
        bool Add(Post post);
        bool Update(Post post);
        bool Delete(Post post);
        bool Save();
    }
}
