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
    public class PostRepositoryTests
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
        public void PostRepository_Add_Should_Return_True_After_Saving()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var post = new Post { UserId = "user1", Text = "Test post" };

            // Act
            var result = repository.Add(post);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void PostRepository_Delete_Should_Return_True_After_Saving()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var post = new Post { UserId = "user1", Text = "Test post" };
            dbContext.Add(post);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(post);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void PostRepository_Update_Should_Return_True_After_Saving()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var post = new Post { UserId = "user1", Text = "Test post" };
            dbContext.Add(post);
            dbContext.SaveChanges();
            post.Text = "Updated text";

            // Act
            var result = repository.Update(post);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task PostRepository_GetAllBySubscription_Should_Return_Posts_For_Subscribed_Users()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };
            dbContext.Users.AddRange(user1, user2);
            await dbContext.SaveChangesAsync();
            var postType = new PostType {Id = 1, Type = "TEST" };
            dbContext.PostTypes.Add(postType);
            await dbContext.SaveChangesAsync();
            var post = new Post { UserId = "user1", Text = "Test post", TypeId = 1 };
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();

            // Act
            var posts = await repository.GetAllBySubscription("user2", new List<string> { "user1" }, new List<int>(), new List<int>(), 1, 10);


            // Assert
            posts.Should().HaveCount(1);
            posts[0].UserId.Should().Be("user1");
        }

        [Fact]
        public async Task PostRepository_GetAllFromProfileByUserId_Should_Return_Posts_For_User()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var user = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            var post = new Post { UserId = "user1", Text = "Test post" };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            // Act
            var posts = await repository.GetAllFromProfileByUserId("user1", 1, 10);

            // Assert
            posts.Should().HaveCount(1);
            posts[0].UserId.Should().Be("user1");
        }

        [Fact]
        public async Task PostRepository_GetAllByProjectId_Should_Return_Posts_For_Project()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var project = new Project { Id = 1, Title = "Test Project" };
            var user = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            dbContext.Users.Add(user);
            dbContext.Projects.Add(project);
            dbContext.SaveChanges();
            var post = new Post { UserId = "user1", ProjectId = 1, Text = "Test project post" };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            // Act
            var posts = await repository.GetAllByProjectId(1, 1, 10);

            // Assert
            posts.Should().HaveCount(1);
            posts[0].ProjectId.Should().Be(1);
        }

        [Fact]
        public async Task PostRepository_GetAllByCommunityId_Should_Return_Posts_For_Community()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var community = new Community { Id = 1, Title = "Test Community" };
            var user = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            dbContext.Users.Add(user);
            dbContext.Communities.Add(community);
            dbContext.SaveChanges();
            var post = new Post { UserId = "user1", CommunityId = 1, Text = "Test community post" };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            // Act
            var posts = await repository.GetAllByCommunityId(1, 1, 10);

            // Assert
            posts.Should().HaveCount(1);
            posts[0].CommunityId.Should().Be(1);
        }

        [Fact]
        public async Task PostRepository_GetByIdAsync_Should_Return_Post_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var image = new Image { Id = 1, ImagePath = "path" };

            dbContext.Users.Add(user);
            dbContext.Images.Add(image);
            await dbContext.SaveChangesAsync(); 

            var post = new Post { Id = 1, UserId = "user1", Text = "Test post", ImageId = 1 };
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(post.Id);
        }


        [Fact]
        public async Task PostRepository_GetAllEmptyAsync_Should_Return_Empty_Posts()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);
            var post = new Post { UserId = "user1", Text = null, ImageId = null };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();

            // Act
            var posts = await repository.GetAllEmptyAsync();

            // Assert
            posts.Should().HaveCount(1);
            posts[0].Text.Should().BeNull();
            posts[0].ImageId.Should().BeNull();
        }

        [Fact]
        public async Task PostRepository_GetAllBySubscription_Should_Return_Posts_From_Projects_When_User_Is_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };
            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var project1 = new Project { Id = 1, Title = "Test Project", IsPrivate = true };
            dbContext.Projects.Add(project1);
            dbContext.SaveChanges();

            var follower = new ProjectFollower { ProjectId = 1, UserId = "user2", IsMember = true };
            dbContext.ProjectFolloweres.Add(follower);
            dbContext.SaveChanges();

            var post1 = new Post { UserId = "user1", Text = "Test post in project", ProjectId = 1 };
            dbContext.Posts.Add(post1);
            dbContext.SaveChanges();

            // Act
            var posts = await repository.GetAllBySubscription("user2", new List<string>(), new List<int> { 1 }, new List<int>(), 1, 10);

            // Assert
            posts.Should().HaveCount(1);
            posts[0].ProjectId.Should().Be(1);
        }

        [Fact]
        public void PostRepository_CommentRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PostRepository_GetAllByProjectId_Should_Return_Posts_With_Limited_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);

            var user = new User { Id = "user1", UserName = "User1", Email = "user1@example.com" };
            var project = new Project { Id = 1, Title = "Test Project" };
            dbContext.Users.Add(user);
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var post1 = new Post { Id = 1, UserId = "user1", ProjectId = 1, Text = "Post 1", CreatedAt = DateTime.UtcNow };
            var post2 = new Post { Id = 2, UserId = "user1", ProjectId = 1, Text = "Post 2", CreatedAt = DateTime.UtcNow.AddMinutes(1) };
            var post3 = new Post { Id = 3, UserId = "user1", ProjectId = 1, Text = "Post 3", CreatedAt = DateTime.UtcNow.AddMinutes(2) };

            dbContext.Posts.AddRange(post1, post2, post3);
            await dbContext.SaveChangesAsync();

            int page = 1;
            int pageSize = 2;
            int lastPostId = 3; // Ожидаем, что вернутся посты с Id < 3 (то есть post1 и post2)

            // Act
            var result = await repository.GetAllByProjectId(1, page, pageSize, lastPostId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainSingle(p => p.Id == 1);
            result.Should().ContainSingle(p => p.Id == 2);
        }

        [Fact]
        public async Task PostRepository_GetAllByCommunityId_Should_Return_Posts_With_Limited_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);

            var user = new User { Id = "user1", UserName = "User1", Email = "user1@example.com" };
            var community = new Community { Id = 1, Title = "Test Community" };
            dbContext.Users.Add(user);
            dbContext.Communities.Add(community);
            await dbContext.SaveChangesAsync();

            var post1 = new Post { Id = 1, UserId = "user1", CommunityId = 1, Text = "Community Post 1", CreatedAt = DateTime.UtcNow };
            var post2 = new Post { Id = 2, UserId = "user1", CommunityId = 1, Text = "Community Post 2", CreatedAt = DateTime.UtcNow.AddMinutes(1) };
            var post3 = new Post { Id = 3, UserId = "user1", CommunityId = 1, Text = "Community Post 3", CreatedAt = DateTime.UtcNow.AddMinutes(2) };

            dbContext.Posts.AddRange(post1, post2, post3);
            await dbContext.SaveChangesAsync();

            int page = 1;
            int pageSize = 2;
            int lastPostId = 3; // Ожидаем, что вернутся посты с Id < 3 (то есть post1 и post2)

            // Act
            var result = await repository.GetAllByCommunityId(1, page, pageSize, lastPostId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainSingle(p => p.Id == 1);
            result.Should().ContainSingle(p => p.Id == 2);
        }

        [Fact]
        public async Task PostRepository_GetAllBySubscription_Should_Return_Posts_With_Limited_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);

            var user = new User { Id = "user1", UserName = "User1", Email = "user1@example.com" };
            var friend = new User { Id = "friend1", UserName = "Friend1", Email = "friend1@example.com" };
            dbContext.Users.AddRange(user, friend);
            await dbContext.SaveChangesAsync();

            var project = new Project { Id = 1, Title = "Test Project", IsPrivate = false };
            var community = new Community { Id = 1, Title = "Test Community" };
            dbContext.Projects.Add(project);
            dbContext.Communities.Add(community);
            await dbContext.SaveChangesAsync();

            var post1 = new Post { Id = 1, UserId = "user1", Text = "User Post 1", CreatedAt = DateTime.UtcNow };
            var post2 = new Post { Id = 2, UserId = "friend1", Text = "Friend Post", CreatedAt = DateTime.UtcNow.AddMinutes(1) };
            var post3 = new Post { Id = 3, UserId = "user1", ProjectId = 1, Text = "Project Post", CreatedAt = DateTime.UtcNow.AddMinutes(2) };
            var post4 = new Post { Id = 4, UserId = "user1", CommunityId = 1, Text = "Community Post", CreatedAt = DateTime.UtcNow.AddMinutes(3) };

            dbContext.Posts.AddRange(post1, post2, post3, post4);
            await dbContext.SaveChangesAsync();

            var friendIds = new List<string> { "friend1" };
            var projectIds = new List<int> { 1 };
            var communityIds = new List<int> { 1 };
            int page = 1;
            int pageSize = 3;
            int lastPostId = 4;

            // Act
            var result = await repository.GetAllBySubscription("user1", friendIds, projectIds, communityIds, page, pageSize, lastPostId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().ContainSingle(p => p.Id == 1);
            result.Should().ContainSingle(p => p.Id == 2);
            result.Should().ContainSingle(p => p.Id == 3);
        }

        [Fact]
        public async Task PostRepository_GetAllFromProfileByUserId_Should_Return_Posts_With_Limited_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new PostRepository(dbContext);

            var user = new User { Id = "user1", UserName = "User1", Email = "user1@example.com" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var post1 = new Post { Id = 1, UserId = "user1", Text = "First Post", CreatedAt = DateTime.UtcNow };
            var post2 = new Post { Id = 2, UserId = "user1", Text = "Second Post", CreatedAt = DateTime.UtcNow.AddMinutes(1) };
            var post3 = new Post { Id = 3, UserId = "user1", Text = "Third Post", CreatedAt = DateTime.UtcNow.AddMinutes(2) };
            var post4 = new Post { Id = 4, UserId = "user1", Text = "Fourth Post", CreatedAt = DateTime.UtcNow.AddMinutes(3) };

            dbContext.Posts.AddRange(post1, post2, post3, post4);
            await dbContext.SaveChangesAsync();

            int page = 1;
            int pageSize = 3;
            int lastPostId = 4;

            // Act
            var result = await repository.GetAllFromProfileByUserId("user1", page, pageSize, lastPostId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().ContainSingle(p => p.Id == 1);
            result.Should().ContainSingle(p => p.Id == 2);
            result.Should().ContainSingle(p => p.Id == 3);
        }


    }
}
