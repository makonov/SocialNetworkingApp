using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<User> _userManager;

        public CommentController(IPostRepository postRepository, ICommentRepository commentRepository, UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int postId)
        {
            var user = HttpContext.User;
            var currentUser = await _userManager.GetUserAsync(user);

            if (currentUser == null) return Unauthorized();

            var comments = await _commentRepository.GetByPostIdAsync(postId);
            var viewModel = new PostCommentsViewModel 
            {
                Comments = comments,
                CurrentUserId = currentUser.Id
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentViewModel viewModel)
        {
            if (!ModelState.IsValid) 
            {
                TempData["Error"] = "Чтобы создать комментарий, введите текст.";
                return RedirectToAction("Index", new { postId = viewModel.PostId });
            }
            
            var user = HttpContext.User;
            var currentUser = await _userManager.GetUserAsync(user);

            if (currentUser == null) return Unauthorized();

            Comment comment = new Comment
            {
                UserId = currentUser.Id,
                PostId = viewModel.PostId,
                Text = viewModel.Text,
                CreatedAt = DateTime.Now
            };
            
            _commentRepository.Add(comment);

            return RedirectToAction("Index", new { viewModel.PostId});
        }

        public async Task<IActionResult> DeleteComment(int commentId, int postId)
        {
            try
            {
                Comment? commentToDelete = await _commentRepository.GetByIdAsync(commentId);
                if (commentToDelete == null) return NotFound();
                _commentRepository.Delete(commentToDelete);
            }
            catch
            {
                TempData["Error"] = "Комментарий не был удален.";
            }
            return RedirectToAction("Index", new { postId });
        }
    }
}
