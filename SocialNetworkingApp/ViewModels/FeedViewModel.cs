using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class FeedViewModel
    {
        public IEnumerable<(Post data, bool isLiked)> Posts { get; set; }
        public string CurrentUserId { get; set; }
    }
}
