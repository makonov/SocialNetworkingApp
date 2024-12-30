using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Models
{
    public class ProjectStatus
    {
        [Key]
        public int Id { get; set; }
        public string Status { get; set; }
    }
}
