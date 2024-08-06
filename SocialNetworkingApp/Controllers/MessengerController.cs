using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    public class MessengerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IMessageRepository _messageRepository;

        public MessengerController(UserManager<User> userManager, IMessageRepository messageRepository)
        {
            _userManager = userManager;
            _messageRepository = messageRepository;
        }


        // GET: MessengerController
        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            MessengerViewModel viewModel = new MessengerViewModel()
            {
                CurrentUserId = user.Id,
                LastMessages = await _messageRepository.GetLastMessagesForUserAsync(user.Id)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ShowDialogue(string userId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();
            
            User? currentInterlocutor = await _userManager.FindByIdAsync(userId);
            if (currentInterlocutor == null) return NotFound();

            DialogueViewModel viewModel = new DialogueViewModel()
            {
                CurrentUserId = user.Id,
                CurrentInterlocutorId = userId,
                CurrentInterlocutorName = currentInterlocutor.FirstName + " " + currentInterlocutor.LastName,
                Messages = await _messageRepository.GetAllByUserIds(user.Id, userId)
            };

            return View(viewModel);
        }

        // GET: MessengerController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MessengerController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MessengerController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MessengerController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MessengerController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MessengerController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MessengerController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
