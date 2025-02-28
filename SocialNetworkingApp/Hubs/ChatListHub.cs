using Microsoft.AspNetCore.SignalR;

namespace SocialNetworkingApp.Hubs
{
    public class ChatListHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                _userConnections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                _userConnections.Remove(userId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateDialogList(string fromUserId, string toUserId, string messageText, DateTime sentAt, string fromUserFirstName, string fromUserLastName, string toUserFirstName, string toUserLastName)
        {
            if (_userConnections.TryGetValue(fromUserId, out var fromConnectionId))
            {
                await Clients.Client(fromConnectionId).SendAsync("UpdateDialogList", fromUserId, toUserId, messageText, sentAt, fromUserFirstName, fromUserLastName, toUserFirstName, toUserLastName);
            }

            if (_userConnections.TryGetValue(toUserId, out var toConnectionId))
            {
                await Clients.Client(toConnectionId).SendAsync("UpdateDialogList", fromUserId, toUserId, messageText, sentAt, fromUserFirstName, fromUserLastName, toUserFirstName, toUserLastName);
            }
        }

    }


}
