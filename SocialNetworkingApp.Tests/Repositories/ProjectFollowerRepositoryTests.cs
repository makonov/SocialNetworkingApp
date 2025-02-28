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
    public class ProjectFollowerRepositoryTests
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
        public void ProjectFollowerRepository_Add_Should_Add_Follower_To_Database()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var projectFollower = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };

            // Act
            var result = repository.Add(projectFollower);

            // Assert
            result.Should().BeTrue();
            var addedFollower = dbContext.ProjectFolloweres.Find(1);
            addedFollower.Should().NotBeNull();
            addedFollower.UserId.Should().Be("user1");
            addedFollower.ProjectId.Should().Be(1);
        }

        [Fact]
        public void ProjectFollowerRepository_Delete_Should_Remove_Follower_From_Database()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var projectFollower = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            dbContext.ProjectFolloweres.Add(projectFollower);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(projectFollower);

            // Assert
            result.Should().BeTrue();
            var deletedFollower = dbContext.ProjectFolloweres.Find(1);
            deletedFollower.Should().BeNull();
        }

        [Fact]
        public async Task ProjectFollowerRepository_GetAllByUserIdAsync_Should_Return_Followers_By_UserId()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var project1 = new Project { Id = 1, Title = "Project 1", Description = "First Project" };
            var project2 = new Project { Id = 2, Title = "Project 2", Description = "Second Project" };
            dbContext.Projects.AddRange(project1, project2);
            await dbContext.SaveChangesAsync();

            var projectFollower1 = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            var projectFollower2 = new ProjectFollower { Id = 2, UserId = "user1", ProjectId = 2, IsMember = true };
            var projectFollower3 = new ProjectFollower { Id = 3, UserId = "user2", ProjectId = 1, IsMember = false };

            dbContext.ProjectFolloweres.AddRange(projectFollower1, projectFollower2, projectFollower3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllByUserIdAsync("user1");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(f => f.UserId == "user1" && f.ProjectId == 1);
            result.Should().Contain(f => f.UserId == "user1" && f.ProjectId == 2);
        }


        [Fact]
        public async Task ProjectFollowerRepository_GetByIdAsync_Should_Return_Follower_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var projectFollower = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            dbContext.ProjectFolloweres.Add(projectFollower);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be("user1");
            result.ProjectId.Should().Be(1);
        }

        [Fact]
        public async Task ProjectFollowerRepository_GetByProjectIdAsync_Should_Return_Followers_By_ProjectId()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var project1 = new Project { Id = 1, Title = "Project 1", Description = "First Project" };
            var project2 = new Project { Id = 2, Title = "Project 2", Description = "Second Project" };
            dbContext.Projects.AddRange(project1, project2);
            await dbContext.SaveChangesAsync();

            var projectFollower1 = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            var projectFollower2 = new ProjectFollower { Id = 2, UserId = "user2", ProjectId = 1, IsMember = false };
            var projectFollower3 = new ProjectFollower { Id = 3, UserId = "user3", ProjectId = 2, IsMember = true };

            dbContext.ProjectFolloweres.AddRange(projectFollower1, projectFollower2, projectFollower3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByProjectIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(f => f.UserId == "user1" && f.ProjectId == 1);
            result.Should().Contain(f => f.UserId == "user2" && f.ProjectId == 1);
        }


        [Fact]
        public async Task ProjectFollowerRepository_GetByUserIdAndProjectIdAsync_Should_Return_Follower()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            // Создание проектов
            var project = new Project { Id = 1, Title = "Project 1", Description = "First Project" };
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            // Создание пользователей (с UserId)
            var user1 = new User { Id = "user1", UserName = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2@example.com" };
            var user3 = new User { Id = "user3", UserName = "user3@example.com" };
            var user4 = new User { Id = "user4", UserName = "user4@example.com" };

            dbContext.Users.AddRange(user1, user2, user3, user4);
            await dbContext.SaveChangesAsync();

            var projectFollower1 = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            var projectFollower2 = new ProjectFollower { Id = 2, UserId = "user2", ProjectId = 1, IsMember = false };
            var projectFollower3 = new ProjectFollower { Id = 3, UserId = "user3", ProjectId = 1, IsMember = true };
            var projectFollower4 = new ProjectFollower { Id = 4, UserId = "user4", ProjectId = 1, IsMember = true };

            dbContext.ProjectFolloweres.AddRange(projectFollower1, projectFollower2, projectFollower3, projectFollower4);
            await dbContext.SaveChangesAsync();

            // Act
            var result1 = await repository.GetByUserIdAndProjectIdAsync("user1", 1);
            var result2 = await repository.GetByUserIdAndProjectIdAsync("user2", 1);
            var result3 = await repository.GetByUserIdAndProjectIdAsync("user3", 1);
            var result4 = await repository.GetByUserIdAndProjectIdAsync("user4", 1);

            // Assert
            result1.Should().NotBeNull();
            result1.UserId.Should().Be("user1");
            result1.ProjectId.Should().Be(1);

            result2.Should().NotBeNull();
            result2.UserId.Should().Be("user2");
            result2.ProjectId.Should().Be(1);

            result3.Should().NotBeNull();
            result3.UserId.Should().Be("user3");
            result3.ProjectId.Should().Be(1);

            result4.Should().NotBeNull();
            result4.UserId.Should().Be("user4");
            result4.ProjectId.Should().Be(1);
        }


        [Fact]
        public async Task ProjectFollowerRepository_IsMember_Should_Return_True_When_Follower_Is_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var projectFollower = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            dbContext.ProjectFolloweres.Add(projectFollower);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.IsMember("user1", 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ProjectFollowerRepository_IsOwner_Should_Return_True_When_Follower_Is_Owner()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var projectFollower = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsOwner = true };
            dbContext.ProjectFolloweres.Add(projectFollower);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.IsOwner("user1", 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ProjectFollowerRepository_Update_Should_Modify_Follower()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var projectFollower = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            dbContext.ProjectFolloweres.Add(projectFollower);
            dbContext.SaveChanges();

            projectFollower.IsMember = false;

            // Act
            var result = repository.Update(projectFollower);

            // Assert
            result.Should().BeTrue();
            var updatedFollower = dbContext.ProjectFolloweres.Find(1);
            updatedFollower.Should().NotBeNull();
            updatedFollower.IsMember.Should().BeFalse();
        }

        [Fact]
        public void ProjectFollowerRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ProjectFollowerRepository_GetMembersByProjectIdAsync_Should_Return_Only_Members()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectFollowerRepository(dbContext);

            var project = new Project { Id = 1, Title = "Project 1", Description = "First Project" };
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var user1 = new User { Id = "user1", UserName = "user1@example.com" };
            var user2 = new User { Id = "user2", UserName = "user2@example.com" };
            var user3 = new User { Id = "user3", UserName = "user3@example.com" };
            var user4 = new User { Id = "user4", UserName = "user4@example.com" };

            dbContext.Users.AddRange(user1, user2, user3, user4);
            await dbContext.SaveChangesAsync();

            var projectFollower1 = new ProjectFollower { Id = 1, UserId = "user1", ProjectId = 1, IsMember = true };
            var projectFollower2 = new ProjectFollower { Id = 2, UserId = "user2", ProjectId = 1, IsMember = false };
            var projectFollower3 = new ProjectFollower { Id = 3, UserId = "user3", ProjectId = 1, IsMember = true };
            var projectFollower4 = new ProjectFollower { Id = 4, UserId = "user4", ProjectId = 1, IsMember = false };

            dbContext.ProjectFolloweres.AddRange(projectFollower1, projectFollower2, projectFollower3, projectFollower4);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetMembersByProjectIdAsync(1);

            // Assert
            result.Should().HaveCount(2); 
            result.Should().Contain(f => f.UserId == "user1");
            result.Should().Contain(f => f.UserId == "user3");
            result.Should().NotContain(f => f.UserId == "user2");
            result.Should().NotContain(f => f.UserId == "user4");
        }

    }
}
