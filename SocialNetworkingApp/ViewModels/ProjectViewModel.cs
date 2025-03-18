using Microsoft.AspNetCore.Mvc.Rendering;
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
        public bool IsFollower {  get; set; }
        public bool IsOwner { get; set; }
        public Project? Project { get; set; }
        public IEnumerable<ProjectChange?> Changes { get; set; }
        public IEnumerable<ProjectAnnouncement> Announcements { get; set; }
        public IEnumerable<ProjectFollower> Members { get; set; }
        public IEnumerable<(Post data, bool isLiked, int likeCount)> Posts { get; set; }
        public int FollowersCount { get; set; }
        public IEnumerable<SelectListItem>? Statuses { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
        public IEnumerable<SelectListItem>? Users { get; set; }
    }
}
