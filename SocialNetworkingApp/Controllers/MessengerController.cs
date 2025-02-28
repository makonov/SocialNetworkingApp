using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class MessengerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserService _userService;
        private const int PageSize = 10;

        public MessengerController(UserManager<User> userManager, IMessageRepository messageRepository, IUserService userService)
        {
            _userManager = userManager;
            _messageRepository = messageRepository;
            _userService = userService;
        }


        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            MessengerViewModel viewModel = new MessengerViewModel()
            {
                CurrentUserId = currentUser.Id,
                LastMessages = await _messageRepository.GetLastMessagesForUserAsync(currentUser.Id)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ShowDialogue(string userId, int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            User? currentInterlocutor = await _userManager.FindByIdAsync(userId);
            if (currentInterlocutor == null) return NotFound();

            var messages = await _messageRepository.GetMessagesByUserIds(currentUser.Id, userId, page, PageSize);
            messages.Reverse();

            DialogueViewModel viewModel = new DialogueViewModel()
            {
                CurrentUserId = currentUser.Id,
                CurrentInterlocutorId = userId,
                CurrentInterlocutorName = currentInterlocutor.FirstName + " " + currentInterlocutor.LastName,
                Messages = messages
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GetMessages(string interlocutorId, string interlocutorName, int page, int lastMessageId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var messages = await _messageRepository.GetMessagesByUserIds(currentUser.Id, interlocutorId, page, PageSize, lastMessageId);
            messages.Reverse();

            DialogueViewModel viewModel = new DialogueViewModel()
            {
                CurrentUserId = currentUser.Id,
                CurrentInterlocutorId = interlocutorId,
                CurrentInterlocutorName = interlocutorName,
                Messages = messages
            };

            return PartialView("~/Views/Messenger/_ShowMessagesPartial.cshtml", viewModel);
        }
    }
}
