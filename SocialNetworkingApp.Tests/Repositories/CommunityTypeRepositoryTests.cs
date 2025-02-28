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
    public class CommunityTypeRepositoryTests
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
        public async Task CommunityTypeRepository_Add_Should_Add_CommunityType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityTypeRepository(dbContext);
            var communityType = new CommunityType { Type = "Type 1" };

            // Act
            var result = repository.Add(communityType);

            // Assert
            result.Should().BeTrue();
            var addedCommunityType = await dbContext.CommunityTypes.FirstOrDefaultAsync(t => t.Type == "Type 1");
            addedCommunityType.Should().NotBeNull();
        }

        [Fact]
        public async Task CommunityTypeRepository_Delete_Should_Remove_CommunityType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityTypeRepository(dbContext);
            var communityType = new CommunityType { Type = "Type 2" };
            await dbContext.CommunityTypes.AddAsync(communityType);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.Delete(communityType);

            // Assert
            result.Should().BeTrue();
            var deletedCommunityType = await dbContext.CommunityTypes.FirstOrDefaultAsync(t => t.Type == "Type 2");
            deletedCommunityType.Should().BeNull();
        }

        [Fact]
        public async Task CommunityTypeRepository_GetByIdAsync_Should_Return_Correct_CommunityType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityTypeRepository(dbContext);
            var communityType = new CommunityType { Type = "Type 3" };
            await dbContext.CommunityTypes.AddAsync(communityType);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(communityType.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(communityType.Id);
            result.Type.Should().Be("Type 3");
        }

        [Fact]
        public async Task CommunityTypeRepository_GetSelectListAsync_Should_Return_SelectListItems()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityTypeRepository(dbContext);
            var communityTypes = new List<CommunityType>
        {
            new CommunityType { Type = "Type 4" },
            new CommunityType { Type = "Type 5" }
        };
            await dbContext.CommunityTypes.AddRangeAsync(communityTypes);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetSelectListAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(item => !string.IsNullOrEmpty(item.Value)).Should().BeTrue();
            result.All(item => !string.IsNullOrEmpty(item.Text)).Should().BeTrue();
        }

        [Fact]
        public async Task CommunityTypeRepository_Update_Should_Modify_CommunityType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityTypeRepository(dbContext);
            var communityType = new CommunityType { Type = "Type 6" };
            await dbContext.CommunityTypes.AddAsync(communityType);
            await dbContext.SaveChangesAsync();

            communityType.Type = "Updated Type 6";

            // Act
            var result = repository.Update(communityType);

            // Assert
            result.Should().BeTrue();
            var updatedCommunityType = await repository.GetByIdAsync(communityType.Id);
            updatedCommunityType.Should().NotBeNull();
            updatedCommunityType!.Type.Should().Be("Updated Type 6");
        }

        [Fact]
        public async Task CommunityTypeRepository_Save_Should_Save_Changes()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityTypeRepository(dbContext);
            var communityType = new CommunityType { Type = "Type 7" };
            dbContext.CommunityTypes.Add(communityType);

            // Act
            var result = repository.Save(); // Принудительное сохранение изменений

            // Assert
            result.Should().BeTrue();
            var savedCommunityType = await dbContext.CommunityTypes.FirstOrDefaultAsync(t => t.Type == "Type 7");
            savedCommunityType.Should().NotBeNull();
        }

        [Fact]
        public void CommunityTypeRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityTypeRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
