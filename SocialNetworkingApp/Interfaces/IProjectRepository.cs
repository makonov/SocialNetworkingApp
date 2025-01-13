using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(int id);
        Task<List<Project>> GetAllAsync();
        bool Add(Project project);
        bool Update(Project project);
        bool Delete(Project project);
        bool Save();
    }
}
