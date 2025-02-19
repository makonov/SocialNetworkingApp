using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CommunityViewModel
    {
        [Required]
        public User? User { get; set; }
        public string? CurrentUserId { get; set; }
        public bool IsCurrentUserMember { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAdmin { get; set; }
        public Community? Community { get; set; }
        public IEnumerable<(Post data, bool isLiked)> Posts { get; set; }
        public int MemberCount { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
        public IEnumerable<CommunityMember> Admins { get; set; }
        public IEnumerable<SelectListItem>? Users { get; set; }
    }
}
