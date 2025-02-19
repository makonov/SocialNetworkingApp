using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Repositories
{
    public class CommunityTypeRepository : ICommunityTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(CommunityType type)
        {
            _context.Add(type);
            return Save();
        }

        public bool Delete(CommunityType type)
        {
            _context.Remove(type);
            return Save();
        }

        public async Task<CommunityType?> GetByIdAsync(int id)
        {
            return await _context.CommunityTypes.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync()
        {
            return await _context.CommunityTypes
                .Select(pt => new SelectListItem
                {
                    Value = pt.Id.ToString(),
                    Text = pt.Type
                }).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(CommunityType type)
        {
            _context.Update(type);
            return Save();
        }
    }
}
