using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.ViewModels
{
    public class MessengerViewModel
    {
        public string? CurrentUserId { get; set; }
        public IEnumerable<Message>? LastMessages {  get; set; } 
    }
}
