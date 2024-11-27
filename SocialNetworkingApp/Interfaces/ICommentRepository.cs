using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetByPostIdAsync(int postId);
        Task<Comment?> GetByIdAsync(int id);
        Task<List<Comment>> GetByPostIdAsync(int postId, int page, int pageSize, int lastCommentId = 0);
        bool Add(Comment comment);
        bool Update(Comment comment);
        bool Delete(Comment comment);
        bool Save();
    }
}
