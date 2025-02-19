using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface ICommunityMemberRepository
    {
        Task<List<CommunityMember?>> GetAllByUserIdAsync(string userId);
        Task<List<CommunityMember>> GetByCommunityIdAsync(int communityId);
        Task<CommunityMember?> GetByUserIdAndCommunityIdAsync(string userId, int communityId);
        Task<List<CommunityMember>> GetAdminsByCommunityIdAsync(int communityId);
        Task<bool> IsMember(string userId, int communityId);
        Task<bool> IsAdmin(string userId, int communityId);
        Task<CommunityMember> GetByIdAsync(int id);
        bool Add(CommunityMember member);
        bool Update(CommunityMember member);
        bool Delete(CommunityMember member);
        bool Save();
    }
}
