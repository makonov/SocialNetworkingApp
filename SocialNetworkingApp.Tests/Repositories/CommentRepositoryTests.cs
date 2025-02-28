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
    public class CommentRepositoryTests
    {
        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Новый контекст для каждого теста
                .Options;
            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task CommentRepository_Add_Should_Save_Comment()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            var comment = new Comment { Text = "Test Comment", CreatedAt = DateTime.UtcNow };

            // Act
            repository.Add(comment);
            await dbContext.SaveChangesAsync(); // Обязательно сохраняем в БД

            // Assert
            var savedComment = await dbContext.Comments.FindAsync(comment.Id);
            savedComment.Should().NotBeNull();
            savedComment!.Text.Should().Be("Test Comment");
        }

        [Fact]
        public async Task CommentRepository_Delete_Should_Remove_Comment()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            var comment = new Comment { Text = "Test Comment", CreatedAt = DateTime.UtcNow };
            repository.Add(comment);
            await dbContext.SaveChangesAsync(); // Сохраняем перед удалением

            // Act
            repository.Delete(comment);
            await dbContext.SaveChangesAsync(); // Удаляем

            // Assert
            var deletedComment = await dbContext.Comments.FindAsync(comment.Id);
            deletedComment.Should().BeNull();
        }

        [Fact]
        public async Task CommentRepository_Update_Should_Change_Comment_Text()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            var comment = new Comment { Text = "Old Text", CreatedAt = DateTime.UtcNow };
            repository.Add(comment);
            await dbContext.SaveChangesAsync();

            // Act
            comment.Text = "Updated Text";
            repository.Update(comment);
            await dbContext.SaveChangesAsync();

            // Assert
            var updatedComment = await dbContext.Comments.FindAsync(comment.Id);
            updatedComment.Should().NotBeNull();
            updatedComment!.Text.Should().Be("Updated Text");
        }

        [Fact]
        public async Task CommentRepository_GetByIdAsync_Should_Return_Correct_Comment()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            // Создаем пользователя, если у Post есть обязательный UserId
            var user = new User { UserName = "TestUser" };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();  // Сначала сохраняем, чтобы получить ID

            var post = new Post { Text = "Test Post", UserId = user.Id };  // Добавляем UserId
            var image = new Image { ImagePath = "test.jpg" };

            var comment = new Comment
            {
                Text = "Test Comment",
                CreatedAt = DateTime.UtcNow,
                Post = post,
                Image = image
            };

            await dbContext.Posts.AddAsync(post);  // Сохраняем Post, т.к. он связан с User
            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(comment.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Text.Should().Be("Test Comment");
            result.Post.Should().NotBeNull();
            result.Post.Text.Should().Be("Test Post");
            result.Image.Should().NotBeNull();
            result.Image.ImagePath.Should().Be("test.jpg");
        }

        [Fact]
        public async Task CommentRepository_GetByPostIdAsync_Should_Return_Comments_By_PostId()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            // Создаем пользователей и посты
            var user = new User { UserName = "TestUser" };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var post = new Post { Text = "Test Post", UserId = user.Id };
            await dbContext.Posts.AddAsync(post);
            await dbContext.SaveChangesAsync();

            var comment1 = new Comment { Text = "Test Comment 1", CreatedAt = DateTime.UtcNow, PostId = post.Id };
            var comment2 = new Comment { Text = "Test Comment 2", CreatedAt = DateTime.UtcNow, PostId = post.Id };

            await dbContext.Comments.AddAsync(comment1);
            await dbContext.Comments.AddAsync(comment2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByPostIdAsync(post.Id);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].Text.Should().Be("Test Comment 1");
            result[1].Text.Should().Be("Test Comment 2");
            result[0].PostId.Should().Be(post.Id);
            result[1].PostId.Should().Be(post.Id);
        }

        [Fact]
        public async Task CommentRepository_GetByPostIdAsync_With_Paging_Should_Return_Correct_Page_Of_Comments()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            var user = new User { UserName = "TestUser" };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var post = new Post { Text = "Test Post", UserId = user.Id };
            await dbContext.Posts.AddAsync(post);
            await dbContext.SaveChangesAsync();

            var comments = new List<Comment>
            {
                new Comment { Text = "Test Comment 1", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 2", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 3", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 4", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 5", CreatedAt = DateTime.UtcNow, PostId = post.Id }
            };

            await dbContext.Comments.AddRangeAsync(comments);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByPostIdAsync(post.Id, 2, 2);  // Страница 2, 2 комментария на странице

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].Text.Should().Be("Test Comment 3");
            result[1].Text.Should().Be("Test Comment 2");
        }

        [Fact]
        public async Task CommentRepository_GetByPostIdAsync_With_LastCommentId_Should_Return_Correct_Comments()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            var user = new User { UserName = "TestUser" };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var post = new Post { Text = "Test Post", UserId = user.Id };
            await dbContext.Posts.AddAsync(post);
            await dbContext.SaveChangesAsync();

            var comments = new List<Comment>
            {
                new Comment { Text = "Test Comment 1", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 2", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 3", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 4", CreatedAt = DateTime.UtcNow, PostId = post.Id },
                new Comment { Text = "Test Comment 5", CreatedAt = DateTime.UtcNow, PostId = post.Id }
            };

            await dbContext.Comments.AddRangeAsync(comments);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByPostIdAsync(post.Id, 1, 2, 3);  // Страница 1, 2 комментария на странице, начиная с комментария с ID 3

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].Text.Should().Be("Test Comment 2");
            result[1].Text.Should().Be("Test Comment 1");
        }

        [Fact]
        public async Task CommentRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommentRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
