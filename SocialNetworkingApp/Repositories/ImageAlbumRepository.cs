using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using System.Xml.Serialization;

namespace SocialNetworkingApp.Repositories
{
    public class ImageAlbumRepository : IImageAlbumRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageAlbumRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(ImageAlbum album)
        {
            _context.Add(album);
            return Save();
        }

        public bool AlbumExists(string title)
        {
            return _context.ImageAlbums.Any(a => a.Name == title);
        }

        public bool Delete(ImageAlbum album)
        {
            _context.Remove(album);
            return Save();
        }

        public Task<List<ImageAlbum>> GetAllByCommunityId(int? communityId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ImageAlbum>> GetAllByProjectAsync(int? projectId)
        {
            return await _context.ImageAlbums.Where(a => a.ProjectId == projectId).ToListAsync();
        }

        public async Task<List<ImageAlbum>> GetAllByUserAsync(string userid)
        {
            return await _context.ImageAlbums.Where(a => a.UserId == userid).ToListAsync();
        }

        public async Task<ImageAlbum> GetByIdAsync(int id)
        {
            return await _context.ImageAlbums.FindAsync(id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(ImageAlbum album)
        {
            _context.Update(album);
            return Save();
        }
    }
}
