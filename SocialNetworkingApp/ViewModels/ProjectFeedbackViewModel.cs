using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class ProjectFeedbackViewModel
    {
        public IEnumerable<(Post data, bool isLiked)> Posts { get; set; }
        public string CurrentUserId { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
    }
}
