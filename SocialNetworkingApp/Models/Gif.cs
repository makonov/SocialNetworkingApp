using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Gif
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("GifAlbum")]
        public int GifAlbumId { get; set; }
        public GifAlbum? Album { get; set; }
        public string? GifPath {  get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
