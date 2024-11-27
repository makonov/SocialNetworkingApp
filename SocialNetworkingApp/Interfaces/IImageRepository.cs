using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IImageRepository
    {
        Task<Image> GetByPathAsync(string path);
        Task<Image> GetByIdAsync(int id);
        Task<List<Image>> GetAllAsync();
        Task<List<Image>> GetByAlbumIdAsync(int id);
        bool Add(Image image);
        bool Update(Image image);
        bool Delete(Image image);
        bool Save();
    }
}
