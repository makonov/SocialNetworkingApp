using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;
using System.Security.Claims;

namespace SocialNetworkingApp.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<List<User>> GetAllUsersExceptCurrentUserAsync(string currentUserId);
        Task<List<User>> FindUsersAsync(FindFriendViewModel viewModel, string currentUserId);
        Task<User?> GetUserByIdAsync(string userId);
    }
}
