using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }
        [ForeignKey("Image")]
        public int? ImageId { get; set; }
        public Image? Image {  get; set; }
        public string? Text { get; set; }
        public int Likes { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Required]
        [ForeignKey("PostType")]
        public int TypeId {  get; set; }
        public PostType? Type { get; set; }
        [ForeignKey("Project")]
        public int? ProjectId { get; set; } 
        public Project? Project { get; set; }

        [ForeignKey("Community")]
        public int? CommunityId { get; set; }
        public Community? Community { get; set; }
    }
}
