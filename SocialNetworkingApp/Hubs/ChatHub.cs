using Microsoft.AspNetCore.SignalR;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private static readonly Dictionary<string, string> _userConnections = new Dictionary<string, string>();

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!_userConnections.ContainsKey(userId))
            {
                _userConnections.Add(userId, Context.ConnectionId);
            }
            else
            {
                _userConnections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            _userConnections.Remove(userId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string fromUserId, string toUserId, string messageText)
        {
            var message = new Message
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Text = messageText,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            if (_userConnections.TryGetValue(toUserId, out var toConnectionId))
            {
                await Clients.Client(toConnectionId).SendAsync("ReceiveMessage", fromUserId, messageText, message.SentAt);
            }

            if (_userConnections.TryGetValue(fromUserId, out var fromConnectionId))
            {
                await Clients.Client(fromConnectionId).SendAsync("ReceiveMessage", fromUserId, messageText, message.SentAt);
            }
        }
    }
}
