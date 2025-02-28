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
    public class ProjectChangeRepositoryTests
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
        public void ProjectChangeRepository_Add_Should_Add_Change_To_Database()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectChangeRepository(dbContext);

            var projectChange = new ProjectChange { Id = 1, ProjectId = 1, ChangeDescription = "Change 1" };

            // Act
            var result = repository.Add(projectChange);

            // Assert
            result.Should().BeTrue();
            var addedChange = dbContext.ProjectChanges.Find(1);
            addedChange.Should().NotBeNull();
            addedChange.ChangeDescription.Should().Be("Change 1");
        }

        [Fact]
        public void ProjectChangeRepository_Delete_Should_Remove_Change_From_Database()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectChangeRepository(dbContext);

            var projectChange = new ProjectChange { Id = 1, ProjectId = 1, ChangeDescription = "Change 1" };
            dbContext.ProjectChanges.Add(projectChange);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(projectChange);

            // Assert
            result.Should().BeTrue();
            var deletedChange = dbContext.ProjectChanges.Find(1);
            deletedChange.Should().BeNull();
        }

        [Fact]
        public async Task ProjectChangeRepository_GetByIdAsync_Should_Return_Change_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectChangeRepository(dbContext);

            var projectChange = new ProjectChange { Id = 1, ProjectId = 1, ChangeDescription = "Change 1" };
            dbContext.ProjectChanges.Add(projectChange);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.ChangeDescription.Should().Be("Change 1");
        }

        [Fact]
        public async Task ProjectChangeRepository_GetByProjectIdAsync_Should_Return_Changes_By_ProjectId()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectChangeRepository(dbContext);

            var projectChange1 = new ProjectChange { Id = 1, ProjectId = 1, ChangeDescription = "Change 1" };
            var projectChange2 = new ProjectChange { Id = 2, ProjectId = 1, ChangeDescription = "Change 2" };
            var projectChange3 = new ProjectChange { Id = 3, ProjectId = 2, ChangeDescription = "Change 3" };

            dbContext.ProjectChanges.AddRange(projectChange1, projectChange2, projectChange3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByProjectIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.ChangeDescription == "Change 1");
            result.Should().Contain(p => p.ChangeDescription == "Change 2");
        }

        [Fact]
        public void ProjectChangeRepository_Update_Should_Modify_Change()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectChangeRepository(dbContext);

            var projectChange = new ProjectChange { Id = 1, ProjectId = 1, ChangeDescription = "Old Description" };
            dbContext.ProjectChanges.Add(projectChange);
            dbContext.SaveChanges();

            projectChange.ChangeDescription = "Updated Description";

            // Act
            var result = repository.Update(projectChange);

            // Assert
            result.Should().BeTrue();
            var updatedChange = dbContext.ProjectChanges.Find(1);
            updatedChange.Should().NotBeNull();
            updatedChange.ChangeDescription.Should().Be("Updated Description");
        }

        [Fact]
        public void ProjectChangeRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectChangeRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
