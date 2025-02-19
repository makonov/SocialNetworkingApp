using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectAnnouncementRepository
    {
        Task<List<ProjectAnnouncement>> GetAllAsync();
        Task<List<ProjectAnnouncement>> GetFilteredAnnouncementsAsync(string? keyExpression, int? projectTypeId);
        Task<List<ProjectAnnouncement>> GetByProjectIdAsync(int projectId);
        Task<ProjectAnnouncement> GetByIdAsync(int id);
        bool Add(ProjectAnnouncement announcement);
        bool Update(ProjectAnnouncement change);
        bool Delete(ProjectAnnouncement announcement);
        bool Save();
    }
}
