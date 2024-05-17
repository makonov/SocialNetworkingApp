using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class FriendsViewModel
    {
        public IEnumerable<Friend?> Friends { get; set; }
        public string CurrentUserId { get; set; }
    }
}
