using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Models
{
    public class ProjectType
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
    }
}
