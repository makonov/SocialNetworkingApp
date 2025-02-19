using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface ICommunityTypeRepository
    {
        Task<CommunityType?> GetByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetSelectListAsync();
        bool Add(CommunityType type);
        bool Update(CommunityType type);
        bool Delete(CommunityType type);
        bool Save();
    }
}
