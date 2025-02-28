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
    public class ProjectStatusRepositoryTests
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
        public void ProjectStatusRepository_Add_Should_Add_ProjectStatus()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectStatusRepository(dbContext);

            var status = new ProjectStatus { Id = 1, Status = "Active" };

            // Act
            var result = repository.Add(status);

            // Assert
            result.Should().BeTrue();
            dbContext.ProjectStatuses.Should().Contain(s => s.Status == "Active");
        }

        [Fact]
        public void ProjectStatusRepository_Delete_Should_Remove_ProjectStatus()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectStatusRepository(dbContext);

            var status = new ProjectStatus { Id = 1, Status = "Inactive" };
            dbContext.ProjectStatuses.Add(status);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(status);

            // Assert
            result.Should().BeTrue();
            dbContext.ProjectStatuses.Should().NotContain(s => s.Status == "Inactive");
        }

        [Fact]
        public async Task ProjectStatusRepository_GetByIdAsync_Should_Return_ProjectStatus_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectStatusRepository(dbContext);

            var status = new ProjectStatus { Id = 1, Status = "Active" };
            dbContext.ProjectStatuses.Add(status);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Active");
        }

        [Fact]
        public async Task ProjectStatusRepository_GetSelectListAsync_Should_Return_SelectListItems()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectStatusRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var status2 = new ProjectStatus { Id = 2, Status = "Inactive" };

            dbContext.ProjectStatuses.AddRange(status1, status2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetSelectListAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Text == "Active" && s.Value == "1");
            result.Should().Contain(s => s.Text == "Inactive" && s.Value == "2");
        }

        [Fact]
        public void ProjectStatusRepository_Update_Should_Update_ProjectStatus()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectStatusRepository(dbContext);

            var status = new ProjectStatus { Id = 1, Status = "Active" };
            dbContext.ProjectStatuses.Add(status);
            dbContext.SaveChanges();

            status.Status = "Completed";

            // Act
            var result = repository.Update(status);

            // Assert
            result.Should().BeTrue();
            dbContext.ProjectStatuses.Should().Contain(s => s.Status == "Completed");
        }

        [Fact]
        public void ProjectStatusRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectStatusRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }

}
