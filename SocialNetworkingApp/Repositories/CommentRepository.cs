using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Comment comment)
        {
            _context.Add(comment);
            return Save();
        }

        public bool Delete(Comment comment)
        {
            _context.Remove(comment);
            return Save();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<List<Comment>> GetByPostIdAsync(int postId)
        {
            return await _context.Comments.Include(c => c.User).Where(c => c.PostId == postId).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(Comment comment)
        {
            _context.Update(comment);
            return Save();
        }
    }
}
