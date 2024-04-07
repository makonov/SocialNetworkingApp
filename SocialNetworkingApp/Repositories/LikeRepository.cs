using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;

        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Like like)
        {
            _context.Add(like);
            return Save();
        }

        public bool Update(Like like)
        {
            _context.Update(like);
            return Save();
        }

        public bool Delete(Like like)
        {
            _context.Remove(like);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;

        }

        public bool IsPostLikedByUser(int postId, string userId)
        {
            return _context.Likes.Any(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<bool> ChangeLikeStatus(int postId, string userId)
        {
            var isLiked = false;
            Like? like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null)
            {
                Add(new Like { PostId = postId, UserId = userId });
                isLiked = true;
            }
            else
            {
                Delete(like);
            }
            return isLiked;
        }
    }
}
