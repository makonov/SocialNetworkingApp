using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class FriendRequest
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? FromUserId { get; set; }
        public User? FromUser { get; set; }
        [ForeignKey("User")]
        public string? ToUserId { get; set; }
        public User? ToUser { get; set; }
    }
}
