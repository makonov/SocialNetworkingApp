using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("PhotoAlbum")]
        public int PhotoAlbumId { get; set; }
        public PhotoAlbum? Album { get; set; }
        string? Image {  get; set; }
        string? Description { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
