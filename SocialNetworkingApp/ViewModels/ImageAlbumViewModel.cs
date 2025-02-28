using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class ImageAlbumViewModel
    {
        public ImageAlbum Album { get; set; }
        public List<Image> Images { get; set; }
        public bool IsProjectMember { get; set; }
        public bool IsCommunityAdmin { get; set; }
        public bool IsOwner {  get; set; }
        public bool IsForbidden { get; set; }
    }
}
