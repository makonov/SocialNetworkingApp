using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class FeedViewModel
    {
        public IEnumerable<(Post data, bool isLiked)> Posts { get; set; }
        public string CurrentUserId { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
        public int? CommunityId { get; set; }
        public string? CommunityTitle { get; set; }
        public bool IsCommunityAdmin { get; set; }
    }
}
