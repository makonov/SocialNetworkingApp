using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class AlbumCatalogueViewModel
    {
        public IEnumerable<ImageAlbum>? Albums { get; set; }
        public bool IsProjectMember { get; set; }
        public bool IsCommunityAdmin { get; set; }
        public bool IsCommunityMember { get; set; }
        public bool IsOwner { get; set; }
        public bool IsCommunity { get; set; }
        public string CurrentUserId { get; set; }
    }
}
