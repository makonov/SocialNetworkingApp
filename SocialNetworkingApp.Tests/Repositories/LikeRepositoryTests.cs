using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Repositories
{
    public class LikeRepositoryTests
    {
        private static ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public void LikeRepository_Add_Should_Add_Like()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            var like = new Like { PostId = post.Id, UserId = user.Id };

            // Act
            var result = repository.Add(like);

            // Assert
            result.Should().BeTrue();
            var addedLike = dbContext.Likes.FirstOrDefault(l => l.PostId == post.Id && l.UserId == user.Id);
            addedLike.Should().NotBeNull();
        }

        [Fact]
        public void LikeRepository_Delete_Should_Remove_Like()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            var like = new Like { PostId = post.Id, UserId = user.Id };
            dbContext.Likes.Add(like);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(like);

            // Assert
            result.Should().BeTrue();
            var deletedLike = dbContext.Likes.FirstOrDefault(l => l.PostId == post.Id && l.UserId == user.Id);
            deletedLike.Should().BeNull();
        }

        [Fact]
        public void LikeRepository_IsPostLikedByUser_Should_Return_True_When_Like_Exists()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            var like = new Like { PostId = post.Id, UserId = user.Id };
            dbContext.Likes.Add(like);
            dbContext.SaveChanges();

            // Act
            var result = repository.IsPostLikedByUser(post.Id, user.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void LikeRepository_IsPostLikedByUser_Should_Return_False_When_Like_Does_Not_Exist()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            // Act
            var result = repository.IsPostLikedByUser(post.Id, user.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task LikeRepository_ChangeLikeStatus_Should_Like_When_Not_Exists()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            // Act
            var result = await repository.ChangeLikeStatus(post.Id, user.Id);

            // Assert
            result.Should().BeTrue();
            var like = await dbContext.Likes.FirstOrDefaultAsync(l => l.PostId == post.Id && l.UserId == user.Id);
            like.Should().NotBeNull();
        }

        [Fact]
        public async Task LikeRepository_ChangeLikeStatus_Should_Unlike_When_Exists()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            var like = new Like { PostId = post.Id, UserId = user.Id };
            dbContext.Likes.Add(like);
            dbContext.SaveChanges();

            // Act
            var result = await repository.ChangeLikeStatus(post.Id, user.Id);

            // Assert
            result.Should().BeFalse();
            var deletedLike = await dbContext.Likes.FirstOrDefaultAsync(l => l.PostId == post.Id && l.UserId == user.Id);
            deletedLike.Should().BeNull();
        }

        [Fact]
        public void LikeRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void LikeRepository_Update_Should_Update_Like()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            var like = new Like { PostId = post.Id, UserId = user.Id };
            dbContext.Likes.Add(like);
            dbContext.SaveChanges();

            // Act
            like.PostId = 2; 
            var result = repository.Update(like);

            // Assert
            result.Should().BeTrue();
            var updatedLike = dbContext.Likes.FirstOrDefault(l => l.PostId == 2 && l.UserId == user.Id);
            updatedLike.Should().NotBeNull();
            updatedLike.PostId.Should().Be(2);
        }

        [Fact]
        public async Task LikeRepository_GetNumberOfLikes_Should_Return_Correct_Count()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new LikeRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var post = new Post { Id = 1, Text = "Test post", UserId = user1.Id };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            var like1 = new Like { PostId = post.Id, UserId = user1.Id };
            var like2 = new Like { PostId = post.Id, UserId = user2.Id };
            dbContext.Likes.AddRange(like1, like2);
            dbContext.SaveChanges();

            // Act
            var likeCount = await repository.GetNumberOfLikes(post.Id);

            // Assert
            likeCount.Should().Be(2);
        }


    }
}
