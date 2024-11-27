using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class AddAlbumViewModel
    {
        [Required(ErrorMessage = "Введите название альбома")]
        [StringLength(50)]
        public string? Title { get; set; }
        public IFormFile? Image { get; set; }
        [StringLength(50)]
        public string? Description { get; set; }
    }
}
