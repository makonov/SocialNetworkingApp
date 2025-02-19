using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<(Project Project, ProjectChange? LastChange, ProjectAnnouncement? LastAnnouncement)>> GetProjectDataList(IEnumerable<Project> projects);
    }
}
