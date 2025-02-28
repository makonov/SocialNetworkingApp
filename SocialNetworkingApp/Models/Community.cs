using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Community
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey("User")]
        public string? OwnerId { get; set; } 
        public User? Owner { get; set; }
        [ForeignKey("CommunityType")]
        public int TypeId { get; set; } 
        public CommunityType? CommunityType { get; set; }
    }

}
