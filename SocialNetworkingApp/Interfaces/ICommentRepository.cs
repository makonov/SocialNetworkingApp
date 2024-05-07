using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetByPostIdAsync(int postId);
        Task<Comment?> GetByIdAsync(int id);
        bool Add(Comment comment);
        bool Update(Comment comment);
        bool Delete(Comment comment);
        bool Save();
    }
}
