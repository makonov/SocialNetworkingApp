using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ImageAlbum")]
        public int ImageAlbumId { get; set; }
        public ImageAlbum? Album { get; set; }
        public string? ImagePath {  get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
