using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Для комментария необходим Id поста")]
        public int PostId { get; set; }
        [Required(ErrorMessage = "Для комментария необходимо ввести текст")]
        public string? Text { get; set; }
    }
}
