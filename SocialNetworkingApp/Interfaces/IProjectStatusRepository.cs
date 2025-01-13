using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectStatusRepository
    {
        Task<ProjectStatus?> GetByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetSelectListAsync();
        bool Add(ProjectStatus status);
        bool Update(ProjectStatus status);
        bool Delete(ProjectStatus status);
        bool Save();
    }
}
