using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectFeedbackRepository
    {
        Task<List<ProjectFeedback>> GetByProjectIdAsync(int projectId);
        Task<ProjectFeedback> GetByIdAsync(int id);
        bool Add(ProjectFeedback feedback);
        bool Update(ProjectFeedback follower);
        bool Delete(ProjectFeedback follower);
        bool Save();
    }
}
