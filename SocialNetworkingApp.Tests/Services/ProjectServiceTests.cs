using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Services
{
    public class ProjectServiceTests
    {
        private ApplicationDbContext _context;
        private ProjectService _projectService;
        private Mock<IUserService> _userServiceMock;

        public ProjectServiceTests()
        {
            SetUpAsync().Wait();
        }

        private static async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        private async Task SetUpAsync()
        {
            _context = await GetDbContext();
            _userServiceMock = new Mock<IUserService>();
            _projectService = new ProjectService(_context, _userServiceMock.Object);
        }

        [Fact]
        public async Task ProjectServiceTests_GetProjectDataList_ShouldReturnCorrectProjectData()
        {
            // Arrange
            await SetUpAsync();

            var userId = "user1";

            var projects = new List<Project>
            {
                new Project { Id = 1, Title = "Public Project", IsPrivate = false },
                new Project { Id = 2, Title = "Private Project", IsPrivate = true }
            };

            var followers = new List<ProjectFollower>
            {
                new ProjectFollower { UserId = userId, ProjectId = 2, IsOwner = true } 
            };

            var changes = new List<ProjectChange>
            {
                new ProjectChange { Id = 1, ProjectId = 1, ChangeDate = new DateTime(2024, 1, 1) },
                new ProjectChange { Id = 2, ProjectId = 2, ChangeDate = new DateTime(2024, 2, 1) }
            };

            var announcements = new List<ProjectAnnouncement>
            {
                new ProjectAnnouncement { Id = 1, ProjectId = 1, CreatedAt = new DateTime(2024, 1, 5) },
                new ProjectAnnouncement { Id = 2, ProjectId = 2, CreatedAt = new DateTime(2024, 2, 5) }
            };

            _context.Projects.AddRange(projects);
            _context.ProjectFolloweres.AddRange(followers);
            _context.ProjectChanges.AddRange(changes);
            _context.ProjectAnnouncements.AddRange(announcements);
            await _context.SaveChangesAsync();

            // Act
            var result = await _projectService.GetProjectDataList(userId, projects);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.Project.Id == 1 && r.LastChange.Id == 1 && r.LastAnnouncement.Id == 1);
            result.Should().Contain(r => r.Project.Id == 2 && r.LastChange.Id == 2 && r.LastAnnouncement.Id == 2);
        }
    }

}
