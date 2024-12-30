using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IPhotoService _photoService;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<User> _userManager;
        private const int PageSize = 10;

        public PostController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            IPhotoService photoService,
            IImageAlbumRepository albumRepository,
            IImageRepository imageRepository,
            ICommentRepository commentRepository,
            UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _photoService = photoService;
            _imageRepository = imageRepository;
            _albumRepository = albumRepository;
            _commentRepository = commentRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id, int page = 1)
        {
            var user = HttpContext.User;
            var currentUser = await _userManager.GetUserAsync(user);

            if (currentUser == null) return Unauthorized();

            var comments = await _commentRepository.GetByPostIdAsync(id, page, PageSize);
            var post = await _postRepository.GetByIdAsync(id);
            var viewModel = new PostCommentsViewModel
            {
                Comments = comments,
                CurrentUserId = currentUser.Id,
                Post = post,
                PostId = id
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostViewModel viewModel)
        {
            if (!ModelState.IsValid || (viewModel.Text == null && viewModel.Image == null && viewModel.ImagePath == null))
            {
                return RedirectToAction("Index", "Feed");
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var post = new Post
            {
                UserId = user.Id,
                Text = viewModel.Text,
                Likes = 0,
                Name = $"{user.LastName} {user.FirstName}",
                CreatedAt = DateTime.Now,
                TypeId = viewModel.PostTypeId,
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
                post.Image = image;
            }

            if (viewModel.ImagePath != null)
            {
                Image image = await _imageRepository.GetByPathAsync(viewModel.ImagePath);
                post.Image = image;
            }

            _postRepository.Add(post);
            return RedirectToAction("Index", viewModel.From);
        }


        [HttpGet]
        public async Task<IActionResult> GetPosts(int page, int lastPostId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, page, PageSize, lastPostId);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                return (p, isLikedByCurrentUser);
            });

            var viewModel = new FeedViewModel
            {
                Posts = postsWithLikeStatus,
                CurrentUserId = user.Id
            };

            return PartialView("~/Views/Shared/_FeedPartial.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(int postId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var isLiked = await _likeRepository.ChangeLikeStatus(postId, user.Id);
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                post.Likes = isLiked ? post.Likes + 1 : post.Likes - 1;
                _postRepository.Update(post);
                return Json(new { success = true, likes = post.Likes });
            }

            return Json(new { success = false, error = "Пост не найден" });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                _postRepository.Delete(post);
                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Пост не найден" });
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(int postId, string text, string existingImage = null, IFormFile inputFile = null)
        {
            if (text == null && inputFile == null && existingImage == null)
            {
                return Json(new { succsess = false, error = "Пост не может быть пустым" });
            }

            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null && user.Id == post.UserId)
            {
                post.UpdatedAt = DateTime.Now;
                post.Text = text;

                if (inputFile != null)
                {
                    var imageAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                    var album = imageAlbums.FirstOrDefault(g => g.Name == "Изображения на стене");

                    string imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                    var imageUploadResult = await _photoService.ReplacePhotoAsync(inputFile, imageDirectory, post.Image != null ? post.Image.ImagePath : null);
                    string? imagePath = imageUploadResult.IsReplacementSuccess ? imageDirectory + "\\" + imageUploadResult.NewFileName : null;


                    Image image = new Image
                    {
                        ImageAlbumId = album.Id,
                        ImagePath = imagePath,
                        CreatedAt = DateTime.Now
                    };
                    _imageRepository.Add(image);
                    post.Image = image;
                }
                else if (existingImage != null)
                {
                    Image image = await _imageRepository.GetByPathAsync(existingImage);
                    post.Image = image;
                }
                else if (post.Image != null)
                {
                    post.Image = null;
                }
                _postRepository.Update(post);
                return Json(new { success = true, imagePath = post.Image != null ? post.Image.ImagePath : null, time = post.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            }
            return Json(new { success = false, error = "Отказано в доступе" });
        }
    }
}
