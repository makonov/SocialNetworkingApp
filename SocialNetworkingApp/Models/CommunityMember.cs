using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SocialNetworkingApp.Models
{
    public class CommunityMember
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Community")]
        public int CommunityId { get; set; }
        public Community? Community { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }
        public bool IsAdmin { get; set; }
    }
}
