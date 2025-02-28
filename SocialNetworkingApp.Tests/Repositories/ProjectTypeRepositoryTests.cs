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
    public class ProjectTypeRepositoryTests
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
        public void ProjectTypeRepository_Add_Should_Add_ProjectType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectTypeRepository(dbContext);
            var type = new ProjectType { Id = 1, Type = "Research" };

            // Act
            var result = repository.Add(type);

            // Assert
            result.Should().BeTrue();
            dbContext.ProjectTypes.Should().Contain(pt => pt.Type == "Research");
        }

        [Fact]
        public void ProjectTypeRepository_Delete_Should_Remove_ProjectType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectTypeRepository(dbContext);
            var type = new ProjectType { Id = 1, Type = "Research" };
            dbContext.ProjectTypes.Add(type);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(type);

            // Assert
            result.Should().BeTrue();
            dbContext.ProjectTypes.Should().NotContain(pt => pt.Type == "Research");
        }

        [Fact]
        public async Task ProjectTypeRepository_GetByIdAsync_Should_Return_ProjectType_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectTypeRepository(dbContext);
            var type = new ProjectType { Id = 1, Type = "Research" };
            dbContext.ProjectTypes.Add(type);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be("Research");
        }

        [Fact]
        public async Task ProjectTypeRepository_GetSelectListAsync_Should_Return_SelectListItems()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectTypeRepository(dbContext);
            var type1 = new ProjectType { Id = 1, Type = "Research" };
            var type2 = new ProjectType { Id = 2, Type = "Development" };
            dbContext.ProjectTypes.AddRange(type1, type2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetSelectListAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(sl => sl.Text == "Research" && sl.Value == "1");
            result.Should().Contain(sl => sl.Text == "Development" && sl.Value == "2");
        }

        [Fact]
        public void ProjectTypeRepository_Update_Should_Update_ProjectType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectTypeRepository(dbContext);
            var type = new ProjectType { Id = 1, Type = "Research" };
            dbContext.ProjectTypes.Add(type);
            dbContext.SaveChanges();

            type.Type = "Updated Research";
            var result = repository.Update(type);

            // Act
            var updatedType = dbContext.ProjectTypes.FirstOrDefault(t => t.Id == 1);

            // Assert
            result.Should().BeTrue();
            updatedType.Should().NotBeNull();
            updatedType.Type.Should().Be("Updated Research");
        }

        [Fact]
        public void ProjectTypeRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectTypeRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
