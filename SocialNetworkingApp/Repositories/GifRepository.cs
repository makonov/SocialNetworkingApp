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
