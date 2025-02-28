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
    public class ProjectRepositoryTests
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
        public async Task ProjectRepository_Add_Should_Add_Project()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.Add(type1);
            await dbContext.SaveChangesAsync();

            var project = new Project { Id = 1, Title = "New Project", StatusId = 1, TypeId = 1 };

            // Act
            var result = repository.Add(project);

            // Assert
            result.Should().BeTrue();
            dbContext.Projects.Should().Contain(p => p.Title == "New Project");
        }

        [Fact]
        public async Task ProjectRepository_Delete_Should_Remove_Project()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.Add(type1);
            await dbContext.SaveChangesAsync();

            var project = new Project { Id = 1, Title = "Project to Delete", StatusId = 1, TypeId = 1 };
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.Delete(project);

            // Assert
            result.Should().BeTrue();
            dbContext.Projects.Should().NotContain(p => p.Title == "Project to Delete");
        }

        [Fact]
        public async Task ProjectRepository_GetAllAsync_Should_Return_All_Projects()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };
            var type2 = new ProjectType { Id = 2, Type = "Development" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.AddRange(type1, type2);
            await dbContext.SaveChangesAsync();

            var project1 = new Project { Id = 1, Title = "Project 1", StatusId = 1, TypeId = 1 };
            var project2 = new Project { Id = 2, Title = "Project 2", StatusId = 1, TypeId = 2 };
            dbContext.Projects.AddRange(project1, project2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Title == "Project 1");
            result.Should().Contain(p => p.Title == "Project 2");
        }

        [Fact]
        public async Task ProjectRepository_GetByIdAsync_Should_Return_Project_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            // Create ProjectStatus and ProjectType
            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.Add(type1);
            await dbContext.SaveChangesAsync();

            var project = new Project { Id = 1, Title = "Project 1", StatusId = 1, TypeId = 1 };
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Project 1");
        }

        [Fact]
        public async Task ProjectRepository_Update_Should_Update_Project()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.Add(type1);
            await dbContext.SaveChangesAsync();

            var project = new Project { Id = 1, Title = "Project 1", StatusId = 1, TypeId = 1 };
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            project.Title = "Updated Project Title";
            var result = repository.Update(project);

            // Act
            var updatedProject = await repository.GetByIdAsync(1);

            // Assert
            result.Should().BeTrue();
            updatedProject.Should().NotBeNull();
            updatedProject.Title.Should().Be("Updated Project Title");
        }


        [Fact]
        public async Task ProjectRepository_GetFilteredProjectsAsync_Should_Return_Projects_Filtered_By_Title()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };
            var type2 = new ProjectType { Id = 2, Type = "Development" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.AddRange(type1, type2);
            await dbContext.SaveChangesAsync();

            var project1 = new Project { Id = 1, Title = "Project 1", StatusId = 1, TypeId = 1 };
            var project2 = new Project { Id = 2, Title = "Project 2", StatusId = 1, TypeId = 2 };
            var project3 = new Project { Id = 3, Title = "Awesome Project", StatusId = 1, TypeId = 1 };

            dbContext.Projects.AddRange(project1, project2, project3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredProjectsAsync("Project", null);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(p => p.Title == "Project 1");
            result.Should().Contain(p => p.Title == "Project 2");
            result.Should().Contain(p => p.Title == "Awesome Project");
        }

        [Fact]
        public async Task ProjectRepository_GetFilteredProjectsAsync_Should_Return_Projects_Filtered_By_TypeId()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };
            var type2 = new ProjectType { Id = 2, Type = "Development" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.AddRange(type1, type2);
            await dbContext.SaveChangesAsync();

            var project1 = new Project { Id = 1, Title = "Project 1", StatusId = 1, TypeId = 1 };
            var project2 = new Project { Id = 2, Title = "Project 2", StatusId = 1, TypeId = 2 };
            var project3 = new Project { Id = 3, Title = "Project 3", StatusId = 1, TypeId = 1 };

            dbContext.Projects.AddRange(project1, project2, project3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredProjectsAsync(null, 1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.TypeId == 1 && p.Title == "Project 1");
            result.Should().Contain(p => p.TypeId == 1 && p.Title == "Project 3");
            result.Should().NotContain(p => p.TypeId == 2);
        }

        [Fact]
        public async Task ProjectRepository_GetFilteredProjectsAsync_Should_Return_Projects_Filtered_By_Title_And_TypeId()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };
            var type2 = new ProjectType { Id = 2, Type = "Development" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.AddRange(type1, type2);
            await dbContext.SaveChangesAsync();

            var project1 = new Project { Id = 1, Title = "Project 1", StatusId = 1, TypeId = 1 };
            var project2 = new Project { Id = 2, Title = "Project 2", StatusId = 1, TypeId = 2 };
            var project3 = new Project { Id = 3, Title = "Project 3", StatusId = 1, TypeId = 1 };
            var project4 = new Project { Id = 4, Title = "Awesome Project", StatusId = 1, TypeId = 1 };

            dbContext.Projects.AddRange(project1, project2, project3, project4);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredProjectsAsync("Project", 1);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(p => p.Title == "Project 1" && p.TypeId == 1);
            result.Should().Contain(p => p.Title == "Project 3" && p.TypeId == 1);
            result.Should().Contain(p => p.Title == "Awesome Project" && p.TypeId == 1);
            result.Should().NotContain(p => p.Title == "Project 2");
        }

        [Fact]
        public async Task ProjectRepository_GetFilteredProjectsAsync_Should_Return_All_Projects_When_No_Filters()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            var status1 = new ProjectStatus { Id = 1, Status = "Active" };
            var type1 = new ProjectType { Id = 1, Type = "Research" };
            var type2 = new ProjectType { Id = 2, Type = "Development" };

            dbContext.ProjectStatuses.Add(status1);
            dbContext.ProjectTypes.AddRange(type1, type2);
            await dbContext.SaveChangesAsync();

            var project1 = new Project { Id = 1, Title = "Project 1", StatusId = 1, TypeId = 1 };
            var project2 = new Project { Id = 2, Title = "Project 2", StatusId = 1, TypeId = 2 };
            var project3 = new Project { Id = 3, Title = "Project 3", StatusId = 1, TypeId = 1 };

            dbContext.Projects.AddRange(project1, project2, project3);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetFilteredProjectsAsync(null, null);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(p => p.Title == "Project 1");
            result.Should().Contain(p => p.Title == "Project 2");
            result.Should().Contain(p => p.Title == "Project 3");
        }


        [Fact]
        public void ProjectRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProjectRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }
    }
}
