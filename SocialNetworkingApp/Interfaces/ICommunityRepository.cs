using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface ICommunityRepository
    {
        Task<Community?> GetByIdAsync(int id);
        Task<List<Community>> GetAllAsync();
        Task<List<Community>> GetFilteredCommunitiesAsync(string title, int? typeId);
        bool Add(Community community);
        bool Update(Community community);
        bool Delete(Community community);
        bool Save();
    }
}
