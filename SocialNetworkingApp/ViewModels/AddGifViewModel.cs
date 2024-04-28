using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class AddGifViewModel
    {
        [Required(ErrorMessage = "Gif должен быть присвоен альбом")]
        public int GifAlbumId { get; set; }
        [Required(ErrorMessage = "Прикрепите gif изображение")]
        public IFormFile? Gif { get; set; }
        public string? Description { get; set; }
    }
}
