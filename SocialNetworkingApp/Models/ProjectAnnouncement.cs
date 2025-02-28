using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Models
{
    public class ProjectAnnouncement
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
