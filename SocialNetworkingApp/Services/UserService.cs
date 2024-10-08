﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            if (!string.IsNullOrWhiteSpace(viewModel.City))
            {
                users = users.Where(u => u.City == viewModel.City);
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

            int skip = (pageNumber - 1) * pageSize;
            users = users.Skip(skip).Take(pageSize);

            return await users.ToListAsync();
        }


        public async Task<List<User>> GetAllUsersExceptCurrentUserAsync(string currentUserId)
        {
            return await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
        }

        public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<User>> GetPagedUsers(string userId, int page, int pageSize)
        {
            var query = _userManager.Users
                .OrderBy(u => u.LastName + " " + u.FirstName)
                .Where(u => u.Id != userId);

            int usersToSkip = (page - 1) * pageSize;
            query = query.Skip(usersToSkip);

            var users = await query.Take(pageSize).ToListAsync();

            return users;
        }
    }
}
