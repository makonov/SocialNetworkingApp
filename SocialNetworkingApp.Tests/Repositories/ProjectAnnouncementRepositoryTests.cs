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
    public class ProjectAnnouncementRepositoryTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task ProjectAnnouncementRepository_Add_Should_Add_Announcement()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var announcement = new ProjectAnnouncement { Id = 1, Description = "Test announcement", ProjectId = 1 };

            // Act
            var result = repository.Add(announcement);
            var savedAnnouncement = await dbContext.ProjectAnnouncements.FirstOrDefaultAsync(a => a.Id == 1);

            // Assert
            result.Should().BeTrue();
            savedAnnouncement.Should().NotBeNull();
            savedAnnouncement.Description.Should().Be("Test announcement");
        }

        [Fact]
        public async Task ProjectAnnouncementRepository_Delete_Should_Remove_Announcement()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var announcement = new ProjectAnnouncement { Id = 1, Description = "To be deleted", ProjectId = 1 };
            dbContext.ProjectAnnouncements.Add(announcement);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.Delete(announcement);
            var deletedAnnouncement = await dbContext.ProjectAnnouncements.FirstOrDefaultAsync(a => a.Id == 1);

            // Assert
            result.Should().BeTrue();
            deletedAnnouncement.Should().BeNull();
        }

        [Fact]
        public async Task ProjectAnnouncementRepository_GetAllAsync_Should_Return_Announcements()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var project1 = new Project { Id = 1, Title = "Project 1"};
            var project2 = new Project { Id = 2, Title = "Project 2"};

            dbContext.Projects.AddRange(project1, project2);
            await dbContext.SaveChangesAsync();

            dbContext.ProjectAnnouncements.AddRange(
                new ProjectAnnouncement { Id = 1, Description = "Ann1", ProjectId = project1.Id },
                new ProjectAnnouncement { Id = 2, Description = "Ann2", ProjectId = project2.Id }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Project.Should().NotBeNull();
            result[1].Project.Should().NotBeNull();
        }


        [Fact]
        public async Task ProjectAnnouncementRepository_GetByIdAsync_Should_Return_Correct_Announcement()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var announcement = new ProjectAnnouncement { Id = 1, Description = "Specific announcement", ProjectId = 1 };
            dbContext.ProjectAnnouncements.Add(announcement);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Description.Should().Be("Specific announcement");
        }

        [Fact]
        public async Task ProjectAnnouncementRepository_GetByProjectIdAsync_Should_Return_Announcements_For_Project()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            dbContext.ProjectAnnouncements.AddRange(
                new ProjectAnnouncement { Id = 1, Description = "Project 1 Announcement", ProjectId = 1 },
                new ProjectAnnouncement { Id = 2, Description = "Project 2 Announcement", ProjectId = 2 },
                new ProjectAnnouncement { Id = 3, Description = "Project 1 Second Announcement", ProjectId = 1 }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByProjectIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(a => a.Id == 1);
            result.Should().ContainSingle(a => a.Id == 3);
        }

        [Fact]
        public async Task ProjectAnnouncementRepository_GetFilteredAnnouncementsAsync_Should_Filter_By_Description()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var project1 = new Project { Id = 1, Title = "Project 1", TypeId = 101 }; 
            dbContext.Projects.Add(project1);
            await dbContext.SaveChangesAsync();

            dbContext.ProjectAnnouncements.AddRange(
                new ProjectAnnouncement { Id = 1, Description = "Match keyword", ProjectId = 1 },
                new ProjectAnnouncement { Id = 2, Description = "No match here", ProjectId = 1 }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredAnnouncementsAsync("keyword", null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
        }


        [Fact]
        public async Task ProjectAnnouncementRepository_GetFilteredAnnouncementsAsync_Should_Filter_By_ProjectType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var project1 = new Project { Id = 1, TypeId = 5 };
            var project2 = new Project { Id = 2, TypeId = 10 };

            dbContext.Projects.AddRange(project1, project2);
            await dbContext.SaveChangesAsync();

            dbContext.ProjectAnnouncements.AddRange(
                new ProjectAnnouncement { Id = 1, Description = "Announcement 1", ProjectId = 1, Project = project1 },
                new ProjectAnnouncement { Id = 2, Description = "Announcement 2", ProjectId = 2, Project = project2 }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredAnnouncementsAsync(null, 5);

            // Assert
            result.Should().HaveCount(1);
            result.First().ProjectId.Should().Be(1);
        }

        [Fact]
        public async Task ProjectAnnouncementRepository_GetFilteredAnnouncementsAsync_Should_Filter_By_Description_And_ProjectType()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var project1 = new Project { Id = 1, TypeId = 5 };
            var project2 = new Project { Id = 2, TypeId = 10 };

            dbContext.Projects.AddRange(project1, project2);
            await dbContext.SaveChangesAsync();

            dbContext.ProjectAnnouncements.AddRange(
                new ProjectAnnouncement { Id = 1, Description = "Match keyword", ProjectId = 1, Project = project1 },
                new ProjectAnnouncement { Id = 2, Description = "No match", ProjectId = 2, Project = project2 }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredAnnouncementsAsync("keyword", 5);

            // Assert
            result.Should().HaveCount(1);
            result.First().ProjectId.Should().Be(1);
        }

        [Fact]
        public void ProjectAnnouncementRepository_Update_Should_Update_Announcement()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            var announcement = new ProjectAnnouncement { Id = 1, Description = "Old description", ProjectId = 1 };
            dbContext.ProjectAnnouncements.Add(announcement);
            dbContext.SaveChanges();

            // Act
            announcement.Description = "New description";
            var result = repository.Update(announcement);
            var updatedAnnouncement = dbContext.ProjectAnnouncements.First(a => a.Id == 1);

            // Assert
            result.Should().BeTrue();
            updatedAnnouncement.Description.Should().Be("New description");
        }

        [Fact]
        public void ProjectAnnouncementRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectAnnouncementRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
