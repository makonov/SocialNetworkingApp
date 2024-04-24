using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class GifAlbumRepository : IGifAlbumRepository
    {
        private readonly ApplicationDbContext _context;

        public GifAlbumRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(GifAlbum album)
        {
            _context.Add(album);
            return Save();
        }

        public bool Delete(GifAlbum album)
        {
            _context.Remove(album);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(GifAlbum album)
        {
            _context.Update(album);
            return Save();
        }
    }
}
