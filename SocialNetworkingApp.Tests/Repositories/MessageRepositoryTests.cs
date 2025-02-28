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
    public class MessageRepositoryTests
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
        public void MessageRepository_Add_Should_Add_Message()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };

            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var message = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "Hello", SentAt = DateTime.Now };

            // Act
            var result = repository.Add(message);

            // Assert
            result.Should().BeTrue();
            var addedMessage = dbContext.Messages.FirstOrDefault(m => m.FromUserId == user1.Id && m.ToUserId == user2.Id);
            addedMessage.Should().NotBeNull();
        }

        [Fact]
        public void MessageRepository_Delete_Should_Remove_Message()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };

            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var message = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "Hello", SentAt = DateTime.Now };
            dbContext.Messages.Add(message);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(message);

            // Assert
            result.Should().BeTrue();
            var deletedMessage = dbContext.Messages.FirstOrDefault(m => m.Id == message.Id);
            deletedMessage.Should().BeNull();
        }

        [Fact]
        public void MessageRepository_Update_Should_Update_Message()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };

            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var message = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "Hello", SentAt = DateTime.Now };
            dbContext.Messages.Add(message);
            dbContext.SaveChanges();

            // Act
            message.Text = "Updated message";
            var result = repository.Update(message);

            // Assert
            result.Should().BeTrue();
            var updatedMessage = dbContext.Messages.FirstOrDefault(m => m.Id == message.Id);
            updatedMessage.Should().NotBeNull();
            updatedMessage.Text.Should().Be("Updated message");
        }

        [Fact]
        public void MessageRepository_GetAllMessagesByUserIds_Should_Return_Messages()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };

            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var message1 = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "Hello", SentAt = DateTime.Now };
            var message2 = new Message { FromUserId = user2.Id, ToUserId = user1.Id, Text = "Hi", SentAt = DateTime.Now.AddMinutes(1) };
            dbContext.Messages.AddRange(message1, message2);
            dbContext.SaveChanges();

            // Act
            var messages = repository.GetAllMessagesByUserIds(user1.Id, user2.Id).Result;

            // Assert
            messages.Should().HaveCount(2);
        }

        [Fact]
        public void MessageRepository_GetMessagesByUserIds_Should_Return_Paginated_Messages()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };

            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var message1 = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "Hello", SentAt = DateTime.Now };
            var message2 = new Message { FromUserId = user2.Id, ToUserId = user1.Id, Text = "Hi", SentAt = DateTime.Now.AddMinutes(1) };
            dbContext.Messages.AddRange(message1, message2);
            dbContext.SaveChanges();

            // Act
            var messages = repository.GetMessagesByUserIds(user1.Id, user2.Id, 1, 1).Result;

            // Assert
            messages.Should().HaveCount(1);
        }

        [Fact]
        public void MessageRepository_GetLastMessagesForUserAsync_Should_Return_Last_Messages()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };

            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var message1 = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "Hello", SentAt = DateTime.Now };
            var message2 = new Message { FromUserId = user2.Id, ToUserId = user1.Id, Text = "Hi", SentAt = DateTime.Now.AddMinutes(1) };
            dbContext.Messages.AddRange(message1, message2);
            dbContext.SaveChanges();

            // Act
            var lastMessages = repository.GetLastMessagesForUserAsync(user1.Id).Result;

            // Assert
            lastMessages.Should().HaveCount(1);
        }

        [Fact]
        public void MessageRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MessageRepository_GetMessagesByUserIds_Should_Apply_LastMessageId_Condition()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1", Email = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2", Email = "user2@example.com" };

            dbContext.Users.AddRange(user1, user2);
            dbContext.SaveChanges();

            var message1 = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "Hello", SentAt = DateTime.Now.AddMinutes(-10) };
            var message2 = new Message { FromUserId = user2.Id, ToUserId = user1.Id, Text = "Hi", SentAt = DateTime.Now.AddMinutes(-5) };
            var message3 = new Message { FromUserId = user1.Id, ToUserId = user2.Id, Text = "How are you?", SentAt = DateTime.Now };

            dbContext.Messages.AddRange(message1, message2, message3);
            dbContext.SaveChanges();

            int lastMessageId = 2; 

            // Act
            var messages = await repository.GetMessagesByUserIds(user1.Id, user2.Id, page: 1, pageSize: 3, lastMessageId: lastMessageId);

            // Assert
            messages.Should().HaveCount(1);  
            messages[0].Id.Should().BeLessThan(lastMessageId); 
        }

        [Fact]
        public async Task MessageRepository_HasUnreadMessages_Should_Return_True_When_User_Has_Unread_Messages()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var userId = "user1";
            dbContext.Messages.AddRange(
                new Message { Id = 1, ToUserId = userId, IsRead = false, Text = "Unread message 1" },
                new Message { Id = 2, ToUserId = userId, IsRead = true, Text = "Read message" }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.HasUnreadMessages(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MessageRepository_HasUnreadMessages_Should_Return_False_When_User_Has_No_Unread_Messages()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new MessageRepository(dbContext);

            var userId = "user1";
            dbContext.Messages.AddRange(
                new Message { Id = 1, ToUserId = userId, IsRead = true, Text = "Read message 1" },
                new Message { Id = 2, ToUserId = userId, IsRead = true, Text = "Read message 2" }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.HasUnreadMessages(userId);

            // Assert
            result.Should().BeFalse();
        }


    }
}
