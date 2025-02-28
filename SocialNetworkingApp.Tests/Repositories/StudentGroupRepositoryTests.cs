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
    public class StudentGroupRepositoryTests
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
        public void StudentGroupRepository_Add_Should_Add_StudentGroup()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new StudentGroupRepository(dbContext);
            var group = new StudentGroup { Id = 1, GroupName = "Group A" };

            // Act
            var result = repository.Add(group);

            // Assert
            result.Should().BeTrue();
            dbContext.StudentGroups.Should().Contain(g => g.GroupName == "Group A");
        }

        [Fact]
        public void StudentGroupRepository_Delete_Should_Remove_StudentGroup()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new StudentGroupRepository(dbContext);
            var group = new StudentGroup { Id = 1, GroupName = "Group A" };
            dbContext.StudentGroups.Add(group);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(group);

            // Assert
            result.Should().BeTrue();
            dbContext.StudentGroups.Should().NotContain(g => g.GroupName == "Group A");
        }

        [Fact]
        public async Task StudentGroupRepository_GetAllAsync_Should_Return_All_StudentGroups()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new StudentGroupRepository(dbContext);
            var group1 = new StudentGroup { Id = 1, GroupName = "Group A" };
            var group2 = new StudentGroup { Id = 2, GroupName = "Group B" };
            dbContext.StudentGroups.AddRange(group1, group2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(g => g.GroupName == "Group A");
            result.Should().Contain(g => g.GroupName == "Group B");
        }

        [Fact]
        public async Task StudentGroupRepository_GetByIdAsync_Should_Return_StudentGroup_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new StudentGroupRepository(dbContext);
            var group = new StudentGroup { Id = 1, GroupName = "Group A" };
            dbContext.StudentGroups.Add(group);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.GroupName.Should().Be("Group A");
        }

        [Fact]
        public void StudentGroupRepository_Update_Should_Update_StudentGroup()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new StudentGroupRepository(dbContext);
            var group = new StudentGroup { Id = 1, GroupName = "Group A" };
            dbContext.StudentGroups.Add(group);
            dbContext.SaveChanges();

            group.GroupName = "Updated Group";
            var result = repository.Update(group);

            // Act
            var updatedGroup = dbContext.StudentGroups.FirstOrDefault(g => g.Id == 1);

            // Assert
            result.Should().BeTrue();
            updatedGroup.Should().NotBeNull();
            updatedGroup.GroupName.Should().Be("Updated Group");
        }

        [Fact]
        public void StudentGroupRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new StudentGroupRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
