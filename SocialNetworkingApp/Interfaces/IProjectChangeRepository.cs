using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectChangeRepository
    {
        Task<List<ProjectChange>> GetByProjectIdAsync(int projectId);
        Task<ProjectChange> GetByIdAsync(int id);
        bool Add(ProjectChange change);
        bool Update(ProjectChange change);
        bool Delete(ProjectChange change);
        bool Save();
    }
}
