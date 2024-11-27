using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class AddImageViewModel
    {
        [Required(ErrorMessage = "Изображению должен быть присвоен альбом")]
        public int ImageAlbumId { get; set; }
        [Required(ErrorMessage = "Прикрепите изображение")]
        public IFormFile? Image { get; set; }
        [StringLength(100)]
        public string? Description { get; set; }
    }
}
