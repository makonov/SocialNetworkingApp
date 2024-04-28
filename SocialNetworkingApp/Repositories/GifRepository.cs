using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class GifRepository : IGifRepository
    {
        private readonly ApplicationDbContext _context;

        public GifRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Gif gif)
        {
            _context.Add(gif);
            return Save();
        }

        public bool Delete(Gif gif)
        {
            _context.Remove(gif);
            return Save();
        }

        public async Task<Gif> GetByIdAsync(int id)
        {
            return await _context.Gifs.FindAsync(id);
        } 

        public async Task<List<Gif>> GetAllAsync()
        {
            return await _context.Gifs.ToListAsync();
        }

        public async Task<List<Gif>> GetByAlbumIdAsync(int id)
        {
            return await _context.Gifs.Where(g => g.GifAlbumId == id).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(Gif gif)
        {
            _context.Update(gif);
            return Save();
        }
    }
}
