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
    public class FriendRequestRepositoryTests
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
        public async Task FriendRequestRepository_Add_Should_Add_Request()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRequestRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var request = new FriendRequest
            {
                FromUserId = "user1",
                ToUserId = "user2"
            };

            // Act
            var result = repository.Add(request);

            // Assert
            result.Should().BeTrue();
            var addedRequest = await dbContext.FriendRequests
                .FirstOrDefaultAsync(r => r.FromUserId == "user1" && r.ToUserId == "user2");
            addedRequest.Should().NotBeNull();
        }

        [Fact]
        public async Task FriendRequestRepository_Delete_Should_Remove_Request()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRequestRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var request = new FriendRequest
            {
                FromUserId = "user1",
                ToUserId = "user2"
            };
            await dbContext.FriendRequests.AddAsync(request);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.Delete(request);

            // Assert
            result.Should().BeTrue();
            var deletedRequest = await dbContext.FriendRequests
                .FirstOrDefaultAsync(r => r.FromUserId == "user1" && r.ToUserId == "user2");
            deletedRequest.Should().BeNull();
        }

        [Fact]
        public async Task FriendRequestRepository_GetRequest_Should_Return_Request()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRequestRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var request = new FriendRequest
            {
                FromUserId = "user1",
                ToUserId = "user2"
            };
            await dbContext.FriendRequests.AddAsync(request);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.GetRequest("user1", "user2");

            // Assert
            result.Should().NotBeNull();
            result.FromUserId.Should().Be("user1");
            result.ToUserId.Should().Be("user2");
        }

        [Fact]
        public async Task FriendRequestRepository_GetRequestsByReceiverId_Should_Return_Requests()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRequestRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            var user3 = new User { Id = "user3", UserName = "user3" };
            await dbContext.Users.AddRangeAsync(user1, user2, user3);
            await dbContext.SaveChangesAsync();

            var request1 = new FriendRequest { FromUserId = "user1", ToUserId = "user2" };
            var request2 = new FriendRequest { FromUserId = "user3", ToUserId = "user2" };
            await dbContext.FriendRequests.AddRangeAsync(request1, request2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetRequestsByReceiverId("user2");

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.ToUserId == "user2").Should().BeTrue();
        }

        [Fact]
        public async Task FriendRequestRepository_RequestExists_Should_Return_True_If_Exists()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRequestRepository(dbContext);

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            var request = new FriendRequest
            {
                FromUserId = "user1",
                ToUserId = "user2"
            };
            await dbContext.FriendRequests.AddAsync(request);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.RequestExists("user1", "user2");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task FriendRequestRepository_Update_Should_Modify_Request()
        {
            // Arrange
            var dbContext = GetDbContext(); 
            var repository = new FriendRequestRepository(dbContext); 

            var user1 = new User { Id = "user1", UserName = "user1" };
            var user2 = new User { Id = "user2", UserName = "user2" };
            await dbContext.Users.AddRangeAsync(user1, user2); 
            await dbContext.SaveChangesAsync(); 

            var request = new FriendRequest
            {
                FromUserId = "user1",
                ToUserId = "user2"
            };
            await dbContext.FriendRequests.AddAsync(request);
            await dbContext.SaveChangesAsync(); 

            // Act
            request.FromUserId = "user2"; 
            var result = repository.Update(request); 

            // Assert
            result.Should().BeTrue();
            var updatedRequest = await dbContext.FriendRequests
                .FirstOrDefaultAsync(r => r.FromUserId == "user2" && r.ToUserId == "user2"); 
            updatedRequest.Should().NotBeNull(); 
            updatedRequest.FromUserId.Should().Be("user2");
        }


        [Fact]
        public void FriendRequestRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new FriendRequestRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
