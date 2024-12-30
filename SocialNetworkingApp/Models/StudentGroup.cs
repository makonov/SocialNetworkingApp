using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Models
{
    public class StudentGroup
    {
        [Key]
        public int Id { get; set; }
        public string GroupName { get; set; }
    }
}
