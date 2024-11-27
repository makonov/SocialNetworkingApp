using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Для комментария необходим Id поста")]
        public int PostId { get; set; }
        [StringLength(200)]
        public string? Text { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; }
    }
}
