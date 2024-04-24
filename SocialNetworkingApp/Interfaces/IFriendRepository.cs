﻿using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IFriendRepository
    {
        Task<List<string>> GetAllIdsByUserAsync(string userId);
        bool Add(Friend friend);
        bool Update(Friend friend);
        bool Delete(Friend friend);
        bool Save();
    }
}