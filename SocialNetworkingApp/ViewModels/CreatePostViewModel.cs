using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CreatePostViewModel
    {
        [StringLength(5000)]
        public string? Text { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; }
        [Required]
        public string? From { get; set; }
        [Required]
        public int PostTypeId { get; set; }
        public int? ProjectId { get; set; }
        public int? CommunityId { get; set; }
    }
}
