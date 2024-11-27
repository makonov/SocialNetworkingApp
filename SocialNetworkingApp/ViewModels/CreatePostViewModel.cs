using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CreatePostViewModel
    {
        [StringLength(200)]
        public string? Text { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; }
        [Required]
        public string? From { get; set; }
    }
}
