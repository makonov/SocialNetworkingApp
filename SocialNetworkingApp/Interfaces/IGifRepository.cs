using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IGifRepository
    {
        Task<Gif> GetByIdAsync(int id);
        Task<List<Gif>> GetAllAsync();
        Task<List<Gif>> GetByAlbumIdAsync(int id);
        bool Add(Gif gif);
        bool Update(Gif gif);
        bool Delete(Gif gif);
        bool Save();
    }
}
