using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class AlbumCatalogueViewModel
    {
        public IEnumerable<ImageAlbum>? Albums { get; set; }
        public bool IsProjectMember { get; set; }
        public bool IsOwner { get; set; }
    }
}
