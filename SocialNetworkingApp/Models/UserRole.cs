using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        public string? RoleName { get; set; }
    }
}
