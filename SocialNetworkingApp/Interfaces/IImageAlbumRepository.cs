using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IImageAlbumRepository
    {
        Task<List<ImageAlbum>> GetAllByUserAsync(string userid);
        Task<List<ImageAlbum>> GetAllByProjectAsync(int? projectId);
        Task<List<ImageAlbum>> GetAllByCommunityAsync(int? communityId);
        Task<ImageAlbum> GetByIdAsync(int id);
        bool AlbumExists(string title);
        bool Add(ImageAlbum album);
        bool Update(ImageAlbum album);
        bool Delete(ImageAlbum album);
        bool Save();
    }
}
