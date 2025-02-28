using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Post post)
        {
            _context.Add(post);
            return Save();
        }

        public bool Delete(Post post)
        {
            _context.Remove(post);
            return Save();
        }

        public bool Update(Post post)
        {
            _context.Update(post);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        
        public async Task<List<Post>> GetAllBySubscription(string userId, List<string> friendIds, List<int> projectIds, List<int> communityIds, int page, int pageSize, int lastPostId = 0)
        {
            friendIds.Add(userId);

            var query = _context.Posts
               .Include(p => p.User)
               .Include(p => p.Image)
               .Include(p => p.Project)
               .Include(p => p.Community)
               .Where(p => friendIds.Contains(p.UserId) && p.ProjectId == null && p.CommunityId == null);

            var projects = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Image)
                .Include(p => p.Project)
                .Include(p => p.Community)
                .Where(p => p.ProjectId.HasValue && projectIds.Contains(p.ProjectId.Value) && (!p.Project.IsPrivate || _context.ProjectFolloweres.Any(f => f.ProjectId == p.ProjectId && f.UserId == userId && f.IsMember)));


            var communitites = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Image)
                .Include(p => p.Project)
                .Include(p => p.Community)
                .Where(p => p.CommunityId.HasValue && communityIds.Contains(p.CommunityId.Value));

            query = query.Union(projects).Union(communitites).OrderByDescending(p => p.UpdatedAt != default ? p.UpdatedAt : p.CreatedAt);

            int postsToSkip = (page - 1) * pageSize;
            query = query.Skip(postsToSkip);

            if (lastPostId > 0)
            {
                query = query.Where(p => p.Id < lastPostId);
            }

            var posts = await query.Take(pageSize).ToListAsync();

            return posts;
        }

        public async Task<List<Post>> GetAllFromProfileByUserId(string userId, int page, int pageSize, int lastPostId = 0)
        {

            var query = _context.Posts
               .OrderByDescending(p => p.UpdatedAt != default ? p.UpdatedAt : p.CreatedAt)
               .Include(p => p.User)
               .Include(p => p.Image)
               .Where(p => p.UserId == userId && p.ProjectId == null && p.CommunityId == null);

            int postsToSkip = (page - 1) * pageSize;
            query = query.Skip(postsToSkip);

            if (lastPostId > 0)
            {
                query = query.Where(p => p.Id < lastPostId);
            }

            var posts = await query.Take(pageSize).ToListAsync();

            return posts;
        }

        public async Task<List<Post>> GetAllByProjectId(int projectId, int page, int pageSize, int lastPostId = 0)
        {
            var query = _context.Posts
               .OrderByDescending(p => p.UpdatedAt != default ? p.UpdatedAt : p.CreatedAt)
               .Include(p => p.User)
               .Include(p => p.Image)
               .Include(p => p.Project)
               .Where(p => p.ProjectId == projectId);

            int postsToSkip = (page - 1) * pageSize;
            query = query.Skip(postsToSkip);

            if (lastPostId > 0)
            {
                query = query.Where(p => p.Id < lastPostId);
            }

            var posts = await query.Take(pageSize).ToListAsync();

            return posts;
        }

        public async Task<List<Post>> GetAllByCommunityId(int communityId, int page, int pageSize, int lastPostId = 0)
        {
            var query = _context.Posts
               .OrderByDescending(p => p.UpdatedAt != default ? p.UpdatedAt : p.CreatedAt)
               .Include(p => p.User)
               .Include(p => p.Image)
               .Include(p => p.Community)
               .Where(p => p.CommunityId == communityId);

            int postsToSkip = (page - 1) * pageSize;
            query = query.Skip(postsToSkip);

            if (lastPostId > 0)
            {
                query = query.Where(p => p.Id < lastPostId);
            }

            var posts = await query.Take(pageSize).ToListAsync();

            return posts;
        }


        public async Task<Post> GetByIdAsync(int id)
        {
            return await _context.Posts.Include(i => i.Image).Include(u => u.User).FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Post>> GetAllEmptyAsync()
        {
            return await _context.Posts.Where(p => p.Text == null && p.ImageId == null).ToListAsync();
        }
    }
}
