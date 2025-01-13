using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectTypeRepository
    {
        Task<ProjectType?> GetByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetSelectListAsync();
        bool Add(ProjectType type);
        bool Update(ProjectType type);
        bool Delete(ProjectType type);
        bool Save();
    }
}
