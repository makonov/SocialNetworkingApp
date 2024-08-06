using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class DialogueViewModel
    {
        public string? CurrentUserId { get; set; }
        public string? CurrentInterlocutorId { get; set; }
        public string? CurrentInterlocutorName { get; set; }
        public IEnumerable<Message>? Messages { get; set; }
    }
}
