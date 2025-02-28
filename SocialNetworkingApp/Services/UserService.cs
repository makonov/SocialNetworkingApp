using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.ViewModels;
using System.Net;
using System.Security.Claims;

namespace SocialNetworkingApp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<User>> FindUsersPagedAsync(FindFriendViewModel viewModel, string currentUserId, int pageNumber, int pageSize)
        {
            IQueryable<User> users = _userManager.Users.Where(u => u.Id != currentUserId);

            if (!string.IsNullOrWhiteSpace(viewModel.LastName))
            {
                string lastName = viewModel.LastName.Trim();
                users = users.Where(u => u.LastName.Contains(lastName));
            }

            if (!string.IsNullOrWhiteSpace(viewModel.FirstName))
            {
                string firstName = viewModel.FirstName.Trim();
                users = users.Where(u => u.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrWhiteSpace(viewModel.Gender))
            {
                switch (viewModel.Gender)
                {
                    case "Male":
                        users = users.Where(u => u.IsMale == true);
                        break;
                    case "Female":
                        users = users.Where(u => u.IsMale == false);
                        break;
                }
            }

            if (viewModel.FromAge != null && viewModel.ToAge != null && viewModel.FromAge > viewModel.ToAge)
            {
                throw new ArgumentException("Нижняя граница возраста не может быть больше верхней границы");
            }

            if (viewModel.FromAge != null)
            {
                users = users.Where(u => DateTime.Now.Year - u.BirthDate.Year >= viewModel.FromAge);
            }

            if (viewModel.ToAge != null)
            {
                users = users.Where(u => DateTime.Now.Year - u.BirthDate.Year <= viewModel.ToAge);
            }

            if (viewModel.StudentGroupId != null)
            {
                users = users.Where(u => u.GroupId == viewModel.StudentGroupId);
            }

            int skip = (pageNumber - 1) * pageSize;
            users = users.Skip(skip).Take(pageSize);

            var response = (await users.ToListAsync()).Intersect(await _userManager.GetUsersInRoleAsync(UserRoles.User)).ToList();

            return response;
        }


        public async Task<List<User>> GetAllUsersExceptCurrentUserAsync(string currentUserId)
        {
            var query = _userManager.Users.Where(u => u.Id != currentUserId);
            return (await query.ToListAsync()).Intersect(await _userManager.GetUsersInRoleAsync(UserRoles.User)).ToList();
        }

        public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserByUserNameAsync(string userName)
        {
            return await _userManager.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<List<User>> GetPagedUsers(string userId, int page, int pageSize)
        {

            var query = _userManager.Users
                .OrderBy(u => u.LastName + " " + u.FirstName)
                .Where(u => u.Id != userId);

            int usersToSkip = (page - 1) * pageSize;
            query = query.Skip(usersToSkip);

            var users = await query.Take(pageSize).ToListAsync();

            users = users.Intersect(await _userManager.GetUsersInRoleAsync(UserRoles.User)).ToList();

            return users;
        }

        public async Task<List<User>> SearchUsersAsync(string query, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<User>();
            }

            // Разделяем запрос на слова
            var queryParts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<User> users = _userManager.Users.Where(u => u.Id != currentUserId);

            // Если введено только одно слово, ищем по имени или фамилии
            if (queryParts.Length == 1)
            {
                var searchTerm = queryParts[0];
                users = users.Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm));
            }
            // Если введены два или больше слов, ищем по имени и фамилии
            else if (queryParts.Length >= 2)
            {
                var firstName = queryParts[0];
                var lastName = queryParts[1];
                users = users.Where(u => u.FirstName.Contains(firstName) && u.LastName.Contains(lastName));
            }

            var response = (await users.ToListAsync()).Intersect(await _userManager.GetUsersInRoleAsync(UserRoles.User)).ToList();

            return response;
        }

        public async Task<IEnumerable<SelectListItem>> GetSelectListOfUsers()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(UserRoles.User);

            var usersList = usersInRole
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName}" // Защита от null в Group
                })
                .ToList();

            return usersList;
        }
        public async Task<List<User>> SearchUsersAsync(FilterUsersViewModel model)
        {
            IQueryable<User> users = _userManager.Users;

            if (!string.IsNullOrWhiteSpace(model.LastName))
            {
                users = users.Where(u => u.LastName.Contains(model.LastName));
            }

            if (!string.IsNullOrWhiteSpace(model.FirstName))
            {
                users = users.Where(u => u.FirstName.Contains(model.FirstName));
            }

            if (model.GroupId.HasValue)
            {
                users = users.Where(u => u.GroupId == model.GroupId.Value);
            }

            if (!string.IsNullOrWhiteSpace(model.UserRole))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(model.UserRole);
                users = users.Where(u => usersInRole.Contains(u));
            }

            if (model.BirthDate.HasValue)
            {
                users = users.Where(u => u.BirthDate.Date == model.BirthDate.Value.Date);
            }

            return await users.ToListAsync();
        }

    }
}
