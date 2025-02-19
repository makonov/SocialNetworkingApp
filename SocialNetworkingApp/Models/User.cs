using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [ForeignKey("StudentGroup")]
        public int? GroupId { get; set; }
        public StudentGroup? Group { get; set; }
        public DateTime BirthDate { get; set; }
        public bool? IsMale { get; set; }
        public DateTime LastLogin { get; set; }
        public string? ProfilePicture { get; set; }
    }
}