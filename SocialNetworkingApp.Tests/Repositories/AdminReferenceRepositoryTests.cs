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
    public class AdminReferenceRepositoryTests
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
        public async Task AdminReferenceRepository_GetAllAsync_Should_Return_All_Entities()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new AdminReferenceRepository<StudentGroup>(dbContext);

            var group1 = new StudentGroup { Id = 1, GroupName = "Group1" };
            var group2 = new StudentGroup { Id = 2, GroupName = "Group2" };

            dbContext.Set<StudentGroup>().AddRange(group1, group2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result.Should().Contain(g => g.GroupName == "Group1");
            result.Should().Contain(g => g.GroupName == "Group2");
        }

        [Fact]
        public async Task AdminReferenceRepository_GetByIdAsync_Should_Return_Entity_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new AdminReferenceRepository<StudentGroup>(dbContext);

            var group = new StudentGroup { Id = 1, GroupName = "Group1" };
            dbContext.Set<StudentGroup>().Add(group);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.GroupName.Should().Be("Group1");
        }

        [Fact]
        public async Task AdminReferenceRepository_AddAsync_Should_Add_Entity()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new AdminReferenceRepository<StudentGroup>(dbContext);

            var group = new StudentGroup { Id = 1, GroupName = "Group1" };

            // Act
            await repository.AddAsync(group);

            // Assert
            var result = await dbContext.Set<StudentGroup>().FindAsync(1);
            result.Should().NotBeNull();
            result.GroupName.Should().Be("Group1");
        }

        [Fact]
        public async Task AdminReferenceRepository_UpdateAsync_Should_Update_Entity()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new AdminReferenceRepository<StudentGroup>(dbContext);

            var group = new StudentGroup { Id = 1, GroupName = "Group1" };
            dbContext.Set<StudentGroup>().Add(group);
            await dbContext.SaveChangesAsync();

            // Act
            group.GroupName = "Updated Group1";
            await repository.UpdateAsync(group);

            // Assert
            var result = await dbContext.Set<StudentGroup>().FindAsync(1);
            result.Should().NotBeNull();
            result.GroupName.Should().Be("Updated Group1");
        }

        [Fact]
        public async Task AdminReferenceRepository_DeleteAsync_Should_Delete_Entity()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new AdminReferenceRepository<StudentGroup>(dbContext);

            var group = new StudentGroup { Id = 1, GroupName = "Group1" };
            dbContext.Set<StudentGroup>().Add(group);
            await dbContext.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(1);

            // Assert
            var result = await dbContext.Set<StudentGroup>().FindAsync(1);
            result.Should().BeNull();
        }


    }
}
