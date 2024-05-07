using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IGifAlbumRepository
    {
        Task<List<GifAlbum>> GetAllByUserAsync(string userid);
        Task<GifAlbum> GetByIdAsync(int id);
        bool AlbumExists(string title);
        bool Add(GifAlbum album);
        bool Update(GifAlbum album);
        bool Delete(GifAlbum album);
        bool Save();
    }
}
