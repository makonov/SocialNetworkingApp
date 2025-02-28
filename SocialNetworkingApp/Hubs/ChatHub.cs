
using Microsoft.AspNetCore.SignalR;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatListHub> _chatListHubContext;
        private static readonly Dictionary<string, string> _userConnections = new();
        private static readonly Dictionary<string, string> _activeChats = new();
        private static readonly HashSet<string> _messengerViewers = new();

        public ChatHub(ApplicationDbContext context, IHubContext<ChatListHub> chatListHubContext)
        {
            _context = context;
            _chatListHubContext = chatListHubContext;
        }

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
                _activeChats.Remove(userId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(string userId, string interlocutorId)
        {
            _activeChats[userId] = interlocutorId;
        }

        public async Task LeaveChat(string userId)
        {
            _activeChats.Remove(userId);
        }

        //public async Task SendMessage(string fromUserId, string toUserId, string messageText)
        //{
        //    bool isRead = _activeChats.TryGetValue(toUserId, out var activeInterlocutor) && activeInterlocutor == fromUserId;

        //    var message = new Message
        //    {
        //        FromUserId = fromUserId,
        //        ToUserId = toUserId,
        //        Text = messageText,
        //        SentAt = DateTime.Now,
        //        IsRead = isRead
        //    };

        //    _context.Messages.Add(message);
        //    await _context.SaveChangesAsync();

        //    if (_userConnections.TryGetValue(toUserId, out var toConnectionId))
        //    {
        //        await Clients.Client(toConnectionId).SendAsync("ReceiveMessage", fromUserId, messageText, message.SentAt);
        //    }

        //    if (_userConnections.TryGetValue(fromUserId, out var fromConnectionId))
        //    {
        //        await Clients.Client(fromConnectionId).SendAsync("ReceiveMessage", fromUserId, messageText, message.SentAt);
        //    }

        //    await Clients.Group("MessengerPage").SendAsync("UpdateDialogList", fromUserId, toUserId, messageText, message.SentAt);
        //}

        public async Task SendMessage(string fromUserId, string toUserId, string messageText)
        {
            bool isRead = _activeChats.TryGetValue(toUserId, out var activeInterlocutor) && activeInterlocutor == fromUserId;

            var message = new Message
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Text = messageText,
                SentAt = DateTime.Now,
                IsRead = isRead
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var fromUser = await _context.Users.FindAsync(fromUserId);
            var toUser = await _context.Users.FindAsync(toUserId);

            if (_userConnections.TryGetValue(toUserId, out var toConnectionId))
            {
                await Clients.Client(toConnectionId).SendAsync("ReceiveMessage", fromUserId, messageText, message.SentAt);

            }

            if (_userConnections.TryGetValue(fromUserId, out var fromConnectionId))
            {
                await Clients.Client(fromConnectionId).SendAsync("ReceiveMessage", fromUserId, messageText, message.SentAt);
            }

            await _chatListHubContext.Clients.User(fromUser.UserName).SendAsync("UpdateDialogList", fromUserId, toUserId, messageText, message.SentAt, fromUser.FirstName, fromUser.LastName, toUser.FirstName, toUser.LastName);

        }



        public async Task<bool> MarkMessagesAsRead(string fromUserId, string toUserId)
        {
            var messages = _context.Messages
                .Where(m => m.FromUserId == fromUserId && m.ToUserId == toUserId && !m.IsRead)
                .ToList();

            bool anyMessagesMarked = false;

            if (messages.Any())
            {
                foreach (var message in messages)
                {
                    message.IsRead = true;
                    anyMessagesMarked = true;
                }

                await _context.SaveChangesAsync();
            }

            var hasUnreadMessagesInOtherChats = _context.Messages
                .Any(m => !m.IsRead && m.ToUserId == toUserId);

            return anyMessagesMarked && !hasUnreadMessagesInOtherChats;
        }


        public async Task JoinMessenger()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                _messengerViewers.Add(userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "MessengerPage");
            }
        }

        public async Task LeaveMessenger()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                _messengerViewers.Remove(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "MessengerPage");
            }
        }


    }



}
