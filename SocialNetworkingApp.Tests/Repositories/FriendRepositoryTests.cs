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
    public class FriendRepositoryTests
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
        public async Task FriendRepository_Add_Should_Add_Friend()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var friend = new Friend
            {
                FirstUserId = "user1",
                SecondUserId = "user2"
            };

            // Act
            var result = repository.Add(friend);

            // Assert
            result.Should().BeTrue();
            var addedFriend = await dbContext.Friends.FirstOrDefaultAsync(f => f.FirstUserId == "user1" && f.SecondUserId == "user2");
            addedFriend.Should().NotBeNull();
        }

        [Fact]
        public async Task FriendRepository_Delete_Should_Remove_Friend()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var friend = new Friend
            {
                FirstUserId = "user1",
                SecondUserId = "user2"
            };
            await dbContext.Friends.AddAsync(friend);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.Delete(friend);

            // Assert
            result.Should().BeTrue();
            var deletedFriend = await dbContext.Friends.FirstOrDefaultAsync(f => f.FirstUserId == "user1" && f.SecondUserId == "user2");
            deletedFriend.Should().BeNull();
        }

        [Fact]
        public async Task FriendRepository_Update_Should_Modify_Friend()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var friend = new Friend
            {
                FirstUserId = "user1",
                SecondUserId = "user2"
            };
            await dbContext.Friends.AddAsync(friend);
            await dbContext.SaveChangesAsync();

            // Act
            friend.SecondUserId = "user3"; // Changing the friend
            var result = repository.Update(friend);

            // Assert
            result.Should().BeTrue();
            var updatedFriend = await dbContext.Friends.FirstOrDefaultAsync(f => f.FirstUserId == "user1" && f.SecondUserId == "user3");
            updatedFriend.Should().NotBeNull();
        }

        [Fact]
        public async Task FriendRepository_Save_Should_Save_Changes()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var friend = new Friend
            {
                FirstUserId = "user1",
                SecondUserId = "user2"
            };
            dbContext.Friends.Add(friend);

            // Act
            var result = repository.Save(); // Save the changes manually

            // Assert
            result.Should().BeTrue();
            var savedFriend = await dbContext.Friends.FirstOrDefaultAsync(f => f.FirstUserId == "user1" && f.SecondUserId == "user2");
            savedFriend.Should().NotBeNull();
        }

        [Fact]
        public async Task FriendRepository_GetByUserId_Should_Return_Friends()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            var user3 = new User { Id = "user3", UserName = "user3" };
            await dbContext.Users.AddRangeAsync(user1, user2, user3);
            await dbContext.SaveChangesAsync();

            var friend1 = new Friend { FirstUserId = "user1", SecondUserId = "user2" };
            var friend2 = new Friend { FirstUserId = "user1", SecondUserId = "user3" };
            await dbContext.Friends.AddRangeAsync(friend1, friend2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserId("user1");

            // Assert
            result.Should().HaveCount(2);
            result.All(f => f.FirstUserId == "user1").Should().BeTrue();
        }

        [Fact]
        public async Task FriendRepository_GetAllIdsByUser_Should_Return_All_FriendIds()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            var user3 = new User { Id = "user3", UserName = "user3" };
            await dbContext.Users.AddRangeAsync(user1, user2, user3);
            await dbContext.SaveChangesAsync();

            var friend1 = new Friend { FirstUserId = "user1", SecondUserId = "user2" };
            var friend2 = new Friend { FirstUserId = "user1", SecondUserId = "user3" };
            await dbContext.Friends.AddRangeAsync(friend1, friend2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllIdsByUserAsync("user1");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("user2");
            result.Should().Contain("user3");
        }

        [Fact]
        public async Task FriendRepository_GetByUserId_With_Pagination_Should_Return_Friends_Paginated()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            var user3 = new User { Id = "user3", UserName = "user3" };
            var user4 = new User { Id = "user4", UserName = "user4" };
            await dbContext.Users.AddRangeAsync(user1, user2, user3, user4);
            await dbContext.SaveChangesAsync();

            var friend1 = new Friend { FirstUserId = "user1", SecondUserId = "user2" };
            var friend2 = new Friend { FirstUserId = "user1", SecondUserId = "user3" };
            var friend3 = new Friend { FirstUserId = "user1", SecondUserId = "user4" };
            await dbContext.Friends.AddRangeAsync(friend1, friend2, friend3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserId("user1", 1, 2); 

            // Assert
            result.Should().HaveCount(2);
            result.First().SecondUserId.Should().Be("user4");
        }

        [Fact]
        public async Task FriendRepository_IsFriend_Should_Return_True_When_Friends_Are_Connected()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var friend = new Friend { FirstUserId = "user1", SecondUserId = "user2" };
            await dbContext.Friends.AddAsync(friend);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.IsFriend("user1", "user2");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void FriendRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task FriendRepository_GetByUserId_With_Pagination_And_LastFriendId_Should_Return_Friends_Limited_By_LastFriendId()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            var user3 = new User { Id = "user3", UserName = "user3" };
            var user4 = new User { Id = "user4", UserName = "user4" };
            await dbContext.Users.AddRangeAsync(user1, user2, user3, user4);
            await dbContext.SaveChangesAsync();

            var friend1 = new Friend { FirstUserId = "user1", SecondUserId = "user2" };
            var friend2 = new Friend { FirstUserId = "user1", SecondUserId = "user3" };
            var friend3 = new Friend { FirstUserId = "user1", SecondUserId = "user4" };
            await dbContext.Friends.AddRangeAsync(friend1, friend2, friend3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserId("user1", 1, 2, friend3.Id); 

            // Assert
            result.Should().HaveCount(2);
            result.All(f => f.FirstUserId == "user1").Should().BeTrue(); 
            result.Last().SecondUserId.Should().Be("user2"); 
            result.First().SecondUserId.Should().Be("user3"); 
        }

        [Fact]
        public async Task FriendRepository_GetByUserId_Should_Return_Friend()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRepository(dbContext);
            var friend = new Friend
            {
                FirstUserId = "user1",
                SecondUserId = "user2"
            };
            dbContext.Friends.Add(friend);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserId("user1", "user2");

            // Assert
            result.Should().NotBeNull();
            result.FirstUserId.Should().Be("user1");
            result.SecondUserId.Should().Be("user2");
        }


    }
}
