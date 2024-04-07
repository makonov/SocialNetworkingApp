using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? FromUserId { get; set; }
        public User? FromUser { get; set; }
        [ForeignKey("User")]
        public string? ToUserId { get; set;}
        public User? ToUser { get; set; }
        public string? Text { get; set; }
        public bool IsRead { get; set; }
        DateTime SentAt { get; set; }
    }
}
