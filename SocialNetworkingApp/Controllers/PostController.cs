﻿
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IPhotoService _photoService;
        private readonly IImageAlbumRepository _albumRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IProjectFollowerRepository _projectFollowerRepository;
        private readonly ICommunityMemberRepository _communityMemberRepository;
        private readonly UserManager<User> _userManager;
        private const int PageSize = 10;

        public PostController(IPostRepository postRepository,
            ILikeRepository likeRepository,
            IFriendRepository friendRepository,
            IPhotoService photoService,
            IImageAlbumRepository albumRepository,
            IImageRepository imageRepository,
            ICommentRepository commentRepository,
            UserManager<User> userManager,
            IProjectFollowerRepository projectFollowerRepository,
            ICommunityMemberRepository communityMemberRepository)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _friendRepository = friendRepository;
            _photoService = photoService;
            _imageRepository = imageRepository;
            _albumRepository = albumRepository;
            _commentRepository = commentRepository;
            _userManager = userManager;
            _projectFollowerRepository = projectFollowerRepository;
            _communityMemberRepository = communityMemberRepository;

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
                Text = viewModel.Text != null ? viewModel.Text : " ",
                Likes = 0,
                Name = $"{user.LastName} {user.FirstName}",
                CreatedAt = DateTime.Now,
                TypeId = viewModel.PostTypeId,
                UpdatedAt = default
            };

            if (viewModel.ProjectId != null) post.ProjectId = viewModel.ProjectId;
            if (viewModel.CommunityId != null) post.CommunityId = viewModel.CommunityId;

            if (viewModel.Image != null)
            {
                
                string imageDirectory = string.Empty; ImageAlbum? album = new ImageAlbum(); List<ImageAlbum> imageAlbums;
                switch (viewModel.PostTypeId)
                {
                    case (int) PostTypeEnum.Profile:
                        imageAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                        album = imageAlbums.FirstOrDefault(g => g.Name == "Изображения на стене");
                        imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                        break;
                    case (int) PostTypeEnum.Project:
                        imageAlbums = await _albumRepository.GetAllByProjectAsync(viewModel.ProjectId);
                        album = imageAlbums.FirstOrDefault(a => a.Name == "Изображения на стене");
                        imageDirectory = $"data\\project-{viewModel.ProjectId}\\{album.Id}";
                        break;
                    case (int)PostTypeEnum.Community:
                        imageAlbums = await _albumRepository.GetAllByCommunityAsync(viewModel.CommunityId);
                        album = imageAlbums.FirstOrDefault(a => a.Name == "Изображения на стене");
                        imageDirectory = $"data\\community-{viewModel.CommunityId}\\{album.Id}";
                        break;
                }
                
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

            switch(viewModel.From)
            {
                case "Project":
                    return RedirectToAction("Details", "Project", new { projectId = viewModel.ProjectId });
                case "Community":
                    return RedirectToAction("Details", "Community", new { communityId = viewModel.CommunityId });
                default:
                    return RedirectToAction("Index", viewModel.From);

            }
        }


        [HttpGet]
        public async Task<IActionResult> GetPosts(int page, int lastPostId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            var friendIds = await _friendRepository.GetAllIdsByUserAsync(user.Id);
            var projectIds = (await _projectFollowerRepository.GetAllByUserIdAsync(user.Id)).Select(p => p.ProjectId).ToList();
            var communityIds = (await _communityMemberRepository.GetAllByUserIdAsync(user.Id)).Select(c => c.CommunityId).ToList();
            var posts = await _postRepository.GetAllBySubscription(user.Id, friendIds, projectIds, communityIds, page, PageSize, lastPostId);

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                int likeCount = _likeRepository.GetNumberOfLikes(p.Id);
                return (p, isLikedByCurrentUser, likeCount);
            });

            var viewModel = new FeedViewModel
            {
                Posts = postsWithLikeStatus,
                CurrentUserId = user.Id
            };

            return PartialView("~/Views/Shared/_FeedPartial.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetProfilePosts(int page, int lastPostId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            List<Post> posts = new List<Post>();
            if (lastPostId > 0)
            {
                string postOwnerId = (await _postRepository.GetByIdAsync(lastPostId)).UserId;
                posts = await _postRepository.GetAllFromProfileByUserId(user.Id, page, PageSize, lastPostId);
            }

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                int likeCount = _likeRepository.GetNumberOfLikes(p.Id);
                return (p, isLikedByCurrentUser, likeCount);
            });

            var viewModel = new FeedViewModel
            {
                Posts = postsWithLikeStatus,
                CurrentUserId = user.Id
            };

            return PartialView("~/Views/Shared/_FeedPartial.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectPosts(int page, int lastPostId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            List<Post> posts = new List<Post>();
            if (lastPostId > 0)
            {
                int? projectId = (await _postRepository.GetByIdAsync(lastPostId)).ProjectId;
                posts = await _postRepository.GetAllByProjectId((int) projectId, page, PageSize, lastPostId);
            }

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                int likeCount = _likeRepository.GetNumberOfLikes(p.Id);
                return (p, isLikedByCurrentUser, likeCount);
            });

            var viewModel = new FeedViewModel
            {
                Posts = postsWithLikeStatus,
                CurrentUserId = user.Id
            };

            return PartialView("~/Views/Shared/_FeedPartial.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetCommunityPosts(int page, int lastPostId)
        {
            var currentUser = HttpContext.User;
            var user = await _userManager.GetUserAsync(currentUser);
            if (user == null) return Unauthorized();

            List<Post> posts = new List<Post>();
            if (lastPostId > 0)
            {
                int? communityId = (await _postRepository.GetByIdAsync(lastPostId)).CommunityId;
                posts = await _postRepository.GetAllByCommunityId((int)communityId, page, PageSize, lastPostId);
            }

            var postsWithLikeStatus = posts.Select(p =>
            {
                bool isLikedByCurrentUser = _likeRepository.IsPostLikedByUser(p.Id, user.Id);
                int likeCount = _likeRepository.GetNumberOfLikes(p.Id);
                return (p, isLikedByCurrentUser, likeCount);
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
                int numberOfLikes = _likeRepository.GetNumberOfLikes(postId);
                return Json(new { success = true, likes = numberOfLikes });
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
                post.Text = text != null ? text : " ";

                if (inputFile != null)
                {
                    string imageDirectory = string.Empty; ImageAlbum? album = new ImageAlbum(); List<ImageAlbum> imageAlbums;
                    if (post.ProjectId != null)
                    {
                        imageAlbums = await _albumRepository.GetAllByProjectAsync(post.ProjectId);
                        album = imageAlbums.FirstOrDefault(a => a.Name == "Изображения на стене");
                        imageDirectory = $"data\\project-{post.ProjectId}\\{album.Id}";
                        
                    }
                    else if (post.CommunityId != null)
                    {
                        imageAlbums = await _albumRepository.GetAllByCommunityAsync(post.CommunityId);
                        album = imageAlbums.FirstOrDefault(a => a.Name == "Изображения на стене");
                        imageDirectory = $"data\\community-{post.CommunityId}\\{album.Id}";
                    }
                    else
                    {
                        imageAlbums = await _albumRepository.GetAllByUserAsync(user.Id);
                        album = imageAlbums.FirstOrDefault(g => g.Name == "Изображения на стене");
                        imageDirectory = $"data\\{user.UserName}\\{album.Id}";
                    }

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
                    int dataIndex = existingImage.IndexOf("data");

                    if (dataIndex != -1)
                    {
                        string relativePath = existingImage.Substring(dataIndex);  

                        Image image = await _imageRepository.GetByPathAsync(relativePath);
                        post.Image = image;  
                    }
                    else
                    {
                        Image image = await _imageRepository.GetByPathAsync(existingImage);
                        post.Image = image;
                    }
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
