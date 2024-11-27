using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Image image)
        {
            _context.Add(image);
            return Save();
        }

        public bool Delete(Image image)
        {
            _context.Remove(image);
            return Save();
        }

        public async Task<Image> GetByIdAsync(int id)
        {
            return await _context.Images.FindAsync(id);
        } 

        public async Task<List<Image>> GetAllAsync()
        {
            return await _context.Images.ToListAsync();
        }

        public async Task<List<Image>> GetByAlbumIdAsync(int id)
        {
            return await _context.Images.Where(i => i.ImageAlbumId == id).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(Image image)
        {
            _context.Update(image);
            return Save();
        }

        public async Task<Image> GetByPathAsync(string path)
        {
            return await _context.Images.FirstOrDefaultAsync(i => i.ImagePath == path);
        }
    }
}
