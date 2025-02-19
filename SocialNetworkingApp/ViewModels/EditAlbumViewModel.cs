using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class EditAlbumViewModel
    {
        [Required(ErrorMessage = "Необходим Id альбома")]
        public int AlbumId { get; set; }
        [Required(ErrorMessage = "Введите название альбома")]
        [StringLength(50)]
        public string? Title { get; set; }
        public IFormFile? Image { get; set; }
        public string? CurrentImage { get; set; }
        [StringLength(50)]
        public string? Description { get; set; }
        public int? ProjectId { get; set; }
        public int? CommunityId { get; set; }
    }
}
