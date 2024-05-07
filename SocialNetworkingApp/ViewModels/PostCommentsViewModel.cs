using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class PostCommentsViewModel
    {
        public int PostId { get; set; } 
        public string? CurrentUserId { get; set; }
        public string? Text { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
