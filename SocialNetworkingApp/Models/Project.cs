using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Goal { get; set; }
        [ForeignKey("ProjectStatus")]
        public int StatusId { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [ForeignKey("ProjectType")]
        public int? TypeId { get; set; }
        public ProjectType? Type { get; set; } 
        public decimal? FundraisingGoal { get; set; } 
        public decimal? FundraisingProgress { get; set; }
        public bool IsPrivate { get; set; }
    }
}
