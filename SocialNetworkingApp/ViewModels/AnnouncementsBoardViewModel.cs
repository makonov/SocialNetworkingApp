using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class AnnouncementsBoardViewModel
    {
        public string? CurrentUserId { get; set; }
        public IEnumerable<ProjectAnnouncement> Announcements { get; set; }
        public string? KeyExpression { get; set; }
        public int? TypeId { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
    }
}
