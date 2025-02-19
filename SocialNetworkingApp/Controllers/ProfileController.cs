using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IPhotoService _photoService;
        private readonly IImageRepository _imageRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly UserManager<User> _userManager;
        private const int pageSize = 10;

        public ProfileController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            IFriendRequestRepository friendRequestRepository,
            IImageAlbumRepository albumRepository,
            IImageRepository imageRepository,
            IPhotoService photoService,
            IUserService userService,
            UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _friendRequestRepository = friendRequestRepository;
            _imageRepository = imageRepository;
            _albumRepository = albumRepository;
            _userService = userService;
            _photoService = photoService;
            _userManager = userManager;
        }

        // GET: ProfileController
        public async Task<IActionResult> Index(string userId = null, int page = 1)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();

            if (userId == null) userId = currentUser.Id;

            var profileUser = await _userService.GetUserByIdAsync(userId);

            var friendIds = new List<string>();
            var posts = await _postRepository.GetAllFromProfileByUserId(userId, page, pageSize);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, currentUser.Id);
                return (p, isLikedByCurrentUser);
            });

            UserStatus status = UserStatus.None;
            if (userId == currentUser.Id)
            {
                status = UserStatus.Owner;
            }
            else if (_friendRepository.IsFriend(userId, currentUser.Id))
            {
                status = UserStatus.Friend;
            }
            else if (_friendRequestRepository.RequestExists(currentUser.Id, userId))
            {
                status = UserStatus.Sender;
            }
            else if (_friendRequestRepository.RequestExists(userId, currentUser.Id))
            {
                status = UserStatus.Reciever;
            }

            var viewModel = new ProfileViewModel
            {
                Posts = postsWithLikeStatus,
                User = profileUser,
                CurrentUserId = currentUser.Id,
                Status = status
            };
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ChooseProfilePicture(ProfileViewModel viewModel)
        {
            var currentUser = await _userService.GetCurrentUserAsync(HttpContext.User);
            if (currentUser == null) return Unauthorized();
            
            if (viewModel.Image != null)
            {
                var imageAlbums = await _albumRepository.GetAllByUserAsync(currentUser.Id);
                var album = imageAlbums.FirstOrDefault(a => a.Name == "Изображения профиля");

                string imageDirectory = $"data\\{currentUser.UserName}\\{album.Id}";
                var imageUploadResult = await _photoService.UploadPhotoAsync(viewModel.Image, imageDirectory);
                string? imagePath = imageUploadResult.IsAttachedAndExtensionValid ? imageDirectory + "\\" + imageUploadResult.FileName : null;

                Image image = new Image
                {
                    ImageAlbumId = album.Id,
                    ImagePath = imagePath,
                    CreatedAt = DateTime.Now
                };

                _imageRepository.Add(image);
                currentUser.ProfilePicture = imagePath;
                await _userManager.UpdateAsync(currentUser);
            }
            else if (viewModel.ImagePath != null)
            {
                currentUser.ProfilePicture = viewModel.ImagePath;
                await _userManager.UpdateAsync(currentUser);
            }
            else
            {
                TempData["Error"] = $"Произошла ошибка при обновлении фотографии профиля: не выбрано изображение";
            }

            return RedirectToAction("Index", new { userId = currentUser.Id, page = 1 });
        }

        // GET: ProfileController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProfileController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProfileController/Create
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

        // GET: ProfileController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProfileController/Edit/5
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

        // GET: ProfileController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProfileController/Delete/5
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
