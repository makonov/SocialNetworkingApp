using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Models
{
    public class PostType
    {
        [Key]
        public int Id { get; set; }
        public string? Type { get; set; }
    }
}
