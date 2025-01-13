using SocialNetworkingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class ProjectViewModel
    {
        [Required]
        public User? User { get; set; }
        public string? CurrentUserId { get; set; }
        public bool IsCurrentUserMember { get; set; }
        public Project? Project { get; set; }
        public IEnumerable<ProjectChange?> Changes { get; set; }
        public IEnumerable<ProjectAnnouncement> Announcements { get; set; }
        public IEnumerable<ProjectFollower> Members { get; set; }
        public IEnumerable<(Post data, bool isLiked)> Posts { get; set; }
        public int FollowersCount { get; set; }
    }
}
