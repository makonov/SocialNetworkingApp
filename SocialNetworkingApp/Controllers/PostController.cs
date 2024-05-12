using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
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
        private readonly IGifAlbumRepository _albumRepository;
        private readonly IGifRepository _gifRepository;
        private readonly UserManager<User> _userManager;
        private const int pageSize = 10;

        public PostController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            IPhotoService photoService,
            IGifAlbumRepository albumRepository,
            IGifRepository gifRepository,
            UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _photoService = photoService;
            _gifRepository = gifRepository;
            _albumRepository = albumRepository;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostViewModel viewModel)
        {
            if (!ModelState.IsValid || (viewModel.Text == null && viewModel.Gif == null && viewModel.GifPath == null))
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
                UpdatedAt = default
            };

            if (viewModel.Gif != null)
            {
                var gifAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                var album = gifAlbums.FirstOrDefault(g => g.Name == "Gif на стене");

                string gifDirectory = $"data\\{user.UserName}\\{album.Id}";
                var gifUploadResult = await _photoService.UploadPhotoAsync(viewModel.Gif, gifDirectory);
                string? gifPath = gifUploadResult.IsAttachedAndExtensionValid ? gifDirectory + "\\" + gifUploadResult.FileName : null;

                Gif gif = new Gif
                {
                    GifAlbumId = album.Id,
                    GifPath = gifPath,
                    CreatedAt = DateTime.Now
                };

                _gifRepository.Add(gif);
                post.Gif = gif;
            }

            if (viewModel.GifPath != null)
            {
                Gif gif = await _gifRepository.GetByPathAsync(viewModel.GifPath);
                post.Gif = gif;
            }

            _postRepository.Add(post);
            return RedirectToAction("Index", "Feed");
        }


        [HttpGet]
        public async Task<IActionResult> GetPosts(int page, int lastPostId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, page, pageSize, lastPostId);

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

            return PartialView("~/Views/Feed/_FeedPartial.cshtml", viewModel);
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
        public async Task<IActionResult> EditPost(int postId, string text, string existingGif = null, IFormFile inputFile = null)
        {
            if (text == null && inputFile == null && existingGif == null)
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
                    var gifAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                    var album = gifAlbums.FirstOrDefault(g => g.Name == "Gif на стене");

                    string gifDirectory = $"data\\{user.UserName}\\{album.Id}";
                    var gifUploadResult = await _photoService.ReplacePhotoAsync(inputFile, gifDirectory, post.Gif.GifPath);
                    string? gifPath = gifUploadResult.IsReplacementSuccess ? gifDirectory + "\\" + gifUploadResult.NewFileName : null;


                    Gif gif = new Gif
                    {
                        GifAlbumId = album.Id,
                        GifPath = gifPath,
                        CreatedAt = DateTime.Now
                    };
                    _gifRepository.Add(gif);
                    post.Gif = gif;
                }
                else if (existingGif != null)
                {
                    Gif gif = await _gifRepository.GetByPathAsync(existingGif);
                    post.Gif = gif;
                }
                else if (post.Gif != null)
                {
                    post.Gif = null;
                }
                _postRepository.Update(post);
                return Json(new { success = true, imagePath = post.Gif.GifPath, time = post.UpdatedAt.ToString("dd.MM.yyyy HH:mm") });
            }
            return Json(new { success = false, error = "Отказано в доступе" });
        }
    }
}
