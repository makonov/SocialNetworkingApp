using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IStudentGroupRepository
    {
        Task<StudentGroup> GetByIdAsync(int id);
        Task<List<StudentGroup>> GetAllAsync();
        bool Add(StudentGroup group);
        bool Update(StudentGroup group);
        bool Delete(StudentGroup group);
        bool Save();
    }
}
