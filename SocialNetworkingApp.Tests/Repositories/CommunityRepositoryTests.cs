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
    public class CommunityRepositoryTests
    {
        private static async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task CommunityRepository_Add_Should_Save_Community()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);
            var community = new Community { Title = "Community 1", TypeId = 1 };

            // Act
            repository.Add(community);
            await dbContext.SaveChangesAsync();

            // Assert
            var savedCommunity = await dbContext.Communities.FindAsync(community.Id);
            savedCommunity.Should().NotBeNull();
            savedCommunity!.Title.Should().Be("Community 1");
            savedCommunity.TypeId.Should().Be(1);
        }

        [Fact]
        public async Task CommunityRepository_Delete_Should_Remove_Community()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);
            var community = new Community { Title = "Community 2", TypeId = 2 };

            repository.Add(community);
            await dbContext.SaveChangesAsync();

            // Act
            repository.Delete(community);
            await dbContext.SaveChangesAsync();

            // Assert
            var deletedCommunity = await dbContext.Communities.FindAsync(community.Id);
            deletedCommunity.Should().BeNull();
        }

        [Fact]
        public async Task CommunityRepository_GetAllAsync_Should_Return_All_Communities()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);

            var communityTypes = new List<CommunityType>
            {
                new CommunityType { Id = 3, Type = "Type 1" },
                new CommunityType { Id = 4, Type = "Type 2" }
            };

            await dbContext.CommunityTypes.AddRangeAsync(communityTypes);
            await dbContext.SaveChangesAsync();

            var communities = new List<Community>
            {
                new Community { Title = "Community 3", TypeId = 3 },
                new Community { Title = "Community 4", TypeId = 4 }
            };

            await dbContext.Communities.AddRangeAsync(communities);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(c => c.Title.Contains("Community")).Should().BeTrue();
        }

        [Fact]
        public async Task CommunityRepository_GetByIdAsync_Should_Return_Correct_Community()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);
            var communityType = new CommunityType { Id = 5, Type = "Type 5" };
            await dbContext.CommunityTypes.AddAsync(communityType);
            await dbContext.SaveChangesAsync();

            var community = new Community { Title = "Community 5", TypeId = 5 };
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(community.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(community.Id);
            result.Title.Should().Be("Community 5");
            result.TypeId.Should().Be(5);  // Проверяем, что TypeId верный
        }


        [Fact]
        public async Task CommunityRepository_GetFilteredCommunitiesAsync_Should_Return_Filtered_Communities()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);
            var communityType1 = new CommunityType { Id = 1, Type = "Type 1" };
            var communityType2 = new CommunityType { Id = 2, Type = "Type 2" };
            await dbContext.CommunityTypes.AddRangeAsync(communityType1, communityType2);
            await dbContext.SaveChangesAsync();

            var communities = new List<Community>
            {
                new Community { Title = "Tech Community", TypeId = 1 },
                new Community { Title = "Art Community", TypeId = 2 }
            };
            await dbContext.Communities.AddRangeAsync(communities);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredCommunitiesAsync("Tech", null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Contain("Tech");
            result.First().TypeId.Should().Be(1);  // Проверяем правильность фильтрации по типу
        }


        [Fact]
        public async Task CommunityRepository_Update_Should_Modify_Existing_Community()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);
            var communityType = new CommunityType { Id = 6, Type = "Type 6" };
            await dbContext.CommunityTypes.AddAsync(communityType);
            await dbContext.SaveChangesAsync();

            var community = new Community { Title = "Community 6", TypeId = 6 };
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            community.Title = "Updated Community 6";

            // Act
            var updated = repository.Update(community);
            await dbContext.SaveChangesAsync();

            var updatedCommunity = await repository.GetByIdAsync(community.Id);

            // Assert
            updated.Should().BeTrue();
            updatedCommunity.Should().NotBeNull();
            updatedCommunity!.Title.Should().Be("Updated Community 6");
            updatedCommunity.TypeId.Should().Be(6);  // Проверка на сохранение правильного типа
        }


        [Fact]
        public async Task CommunityRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CommunityRepository_GetFilteredCommunitiesAsync_Should_Return_Filtered_Communities_By_TypeId()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CommunityRepository(dbContext);

            var communityType1 = new CommunityType { Id = 1, Type = "Type 1" };
            var communityType2 = new CommunityType { Id = 2, Type = "Type 2" };
            await dbContext.CommunityTypes.AddRangeAsync(communityType1, communityType2);
            await dbContext.SaveChangesAsync();

            var communities = new List<Community>
            {
                new Community { Title = "Tech Community", TypeId = 1 },
                new Community { Title = "Art Community", TypeId = 2 },
                new Community { Title = "Tech News", TypeId = 1 }
            };
            await dbContext.Communities.AddRangeAsync(communities);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredCommunitiesAsync("Tech", 1); 

            // Assert
            result.Should().HaveCount(2);
            result.All(c => c.Title.Contains("Tech")).Should().BeTrue(); 
            result.All(c => c.TypeId == 1).Should().BeTrue(); 
        }

    }
}
