using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IGifAlbumRepository
    {
        bool Add(GifAlbum album);
        bool Update(GifAlbum album);
        bool Delete(GifAlbum album);
        bool Save();
    }
}
