using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System.Security.Claims;

namespace SocialNetworkingApp.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetPagedUsers(string userId, int page, int pageSize);
        Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<List<User>> GetAllUsersExceptCurrentUserAsync(string currentUserId);
        Task<List<User>> FindUsersPagedAsync(FindFriendViewModel viewModel, string currentUserId, int pageNumber, int pageSize);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByUserNameAsync(string userName);
        Task<List<User>> SearchUsersAsync(string query, string currentUserId);
        Task<List<User>> SearchUsersAsync(FilterUsersViewModel model);
        Task<IEnumerable<SelectListItem>> GetSelectListOfUsers();
    }
}
