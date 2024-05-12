using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CreatePostViewModel
    {
        [StringLength(200)]
        public string? Text { get; set; }
        public IFormFile? Gif { get; set; }
        public string? GifPath { get; set; }
    }
}
