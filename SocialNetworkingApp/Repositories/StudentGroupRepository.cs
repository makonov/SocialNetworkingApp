using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class StudentGroupRepository : IStudentGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentGroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(StudentGroup group)
        {
            _context.Add(group);
            return Save();
        }

        public bool Delete(StudentGroup group)
        {
            _context.Remove(group);
            return Save();
        }

        public Task<List<StudentGroup>> GetAllAsync()
        {
            return _context.StudentGroups.ToListAsync();
        }

        public async Task<StudentGroup> GetByIdAsync(int id)
        {
            return await _context.StudentGroups.FirstOrDefaultAsync(g => g.Id == id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(StudentGroup group)
        {
            _context.Update(group);
            return Save();
        }
    }
}
