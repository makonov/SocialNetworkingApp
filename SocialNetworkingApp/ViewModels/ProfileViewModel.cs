using SocialNetworkingApp.Data;
using SocialNetworkingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class ProfileViewModel
    {
        public IEnumerable<(Post data, bool isLiked)> Posts { get; set; }
        [Required]
        public User? User { get; set; }
        public string? CurrentUserId { get; set; }
        public UserStatus Status { get; set; }
    }
}
