using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class FriendRequest
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int FromUserId { get; set; }
        public User? FromUser { get; set; }
        [ForeignKey("User")]
        public int ToUserId { get; set; }
        public User? ToUser { get; set; }
    }
}
