using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectFollowerRepository
    {
        Task<List<ProjectFollower?>> GetAllByUserIdAsync(string userId);
        Task<List<ProjectFollower>> GetByProjectIdAsync(int projectId);
        Task<List<ProjectFollower>> GetMembersByProjectIdAsync(int projectId);
        Task<ProjectFollower?> GetByUserIdAndProjectIdAsync(string userId, int  projectId);
        Task<bool> IsMember(string userId, int projectId);
        Task<bool> IsOwner(string userId, int projectId);
        Task<ProjectFollower> GetByIdAsync(int id);
        bool Add(ProjectFollower follower);
        bool Update(ProjectFollower follower);
        bool Delete(ProjectFollower follower);
        bool Save();
    }
}
