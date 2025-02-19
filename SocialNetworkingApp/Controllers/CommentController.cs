using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;
using System.Drawing.Printing;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class CommentController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IPhotoService _photoService;
        private readonly IImageRepository _imageRepository;
        private readonly IUserService _userService;
        private const int PageSize = 10;

        public CommentController(IPostRepository postRepository, 
            ICommentRepository commentRepository, 
            UserManager<User> userManager, 
            IImageAlbumRepository albumRepository, 
            IPhotoService photoService, 
            IImageRepository imageRepository,
            IUserService userService)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _userManager = userManager;
            _albumRepository = albumRepository;
            _photoService = photoService;
            _imageRepository = imageRepository;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentViewModel viewModel)
        {
            if (!ModelState.IsValid || (viewModel.Text == null && viewModel.Image == null && viewModel.ImagePath == null))
            {
                TempData["Error"] = "Чтобы создать комментарий, введите текст или закрепите изображение.";
                return RedirectToAction("Index", new { postId = viewModel.PostId });
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var comment = new Comment
            {
                UserId = user.Id,
                PostId = viewModel.PostId,
                Text = viewModel.Text,
                CreatedAt = DateTime.Now,
                UpdatedAt = default
            };

            if (viewModel.Image != null)
            {
                var imageAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                var album = imageAlbums.FirstOrDefault(g => g.Name == "Изображения на стене");

                string imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Image, imageDirectory);
                string? imagePath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;

                Image image = new Image
                {
                    ImageAlbumId = album.Id,
                    ImagePath = imagePath,
                    CreatedAt = DateTime.Now
                };

                _imageRepository.Add(image);
                comment.Image = image;
            }

            if (viewModel.ImagePath != null)
            {
                Image image = await _imageRepository.GetByPathAsync(viewModel.ImagePath);
                comment.Image = image;
            }

            _commentRepository.Add(comment);
            return RedirectToAction("Details", "Post", new { id = viewModel.PostId });
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(int commentId, string text, string existingImage = null, IFormFile inputFile = null)
        {
            if (text == null && inputFile == null && existingImage == null)
            {
                return Json(new { succsess = false, error = "Комментарий не может быть пустым" });
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment != null && user.Id == comment.UserId)
            {
                comment.UpdatedAt = DateTime.Now;
                comment.Text = text;

                if (inputFile != null)
                {
                    var imageAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                    var album = imageAlbums.FirstOrDefault(g => g.Name == "Изображения на стене");

                    string imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                    var imageUploadResult = await _photoService.ReplacePhotoAsync(inputFile, imageDirectory, comment.Image != null ? comment.Image.ImagePath : null);
                    string? imagePath = imageUploadResult.IsReplacementSuccess ? imageDirectory + "\\" + imageUploadResult.NewFileName : null;


                    Image image = new Image
                    {
                        ImageAlbumId = album.Id,
                        ImagePath = imagePath,
                        CreatedAt = DateTime.Now
                    };
                    _imageRepository.Add(image);
                    comment.Image = image;
                }
                else if (existingImage != null)
                {
                    Image image = await _imageRepository.GetByPathAsync(existingImage);
                    comment.Image = image;
                }
                else if (comment.Image != null)
                {
                    comment.Image = null;
                }
                _commentRepository.Update(comment);
                return Json(new { success = true, imagePath = comment.Image != null ? comment.Image.ImagePath : null, time = comment.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            }
            return Json(new { success = false, error = "Отказано в доступе" });
        }

        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment != null)
            {
                _commentRepository.Delete(comment);
                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Комментарий не найден" });
        }

        public async Task<IActionResult> GetComments(int page, int lastCommentId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            var post = (await _commentRepository.GetByIdAsync(lastCommentId)).Post;
            var comments = await _commentRepository.GetByPostIdAsync(post.Id, page, PageSize, lastCommentId);

            PostCommentsViewModel viewModel = new PostCommentsViewModel
            {
                Comments = comments,
                CurrentUserId = currentUser.Id,
                Post = post,
                PostId = post.Id
            };

            return PartialView("~/Views/Post/_ShowCommentsPartial.cshtml", viewModel);
        }
    }
}
