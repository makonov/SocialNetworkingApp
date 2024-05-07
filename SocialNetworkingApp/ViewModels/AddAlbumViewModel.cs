using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class AddAlbumViewModel
    {
        [Required(ErrorMessage = "Введите название альбома")]
        public string? Title { get; set; }
        public IFormFile? Gif { get; set; }
        public string? Description { get; set; }
    }
}
