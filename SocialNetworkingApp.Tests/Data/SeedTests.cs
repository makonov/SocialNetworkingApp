using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Data
{
    public class SeedTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IApplicationBuilder _applicationBuilder;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceProvider _serviceProvider;

        public SeedTests()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: "TestDatabase"));

            services.AddLogging(); // Добавляем логирование

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
            _serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var appBuilder = new ApplicationBuilder(_serviceProvider);
            _applicationBuilder = appBuilder;
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task SeedUsersAndRolesAsync_ShouldCreateRolesAndUsers_WhenTheyDoNotExist()
        {
            // Act
            await Seed.SeedUsersAndRolesAsync(_applicationBuilder);

            // Assert
            var roles = await _dbContext.Roles.ToListAsync();
            var users = await _dbContext.Users.ToListAsync();

            roles.Should().Contain(r => r.Name == UserRoles.User);
            roles.Should().Contain(r => r.Name == UserRoles.Admin);
            users.Should().Contain(u => u.UserName == "user1");
            users.Should().Contain(u => u.UserName == "admin");
        }

        [Fact]
        public async Task SeedUsersAndRolesAsync_ShouldAddProjectTypes_WhenNoneExist()
        {
            // Act
            await Seed.SeedUsersAndRolesAsync(_applicationBuilder);

            // Assert
            var projectTypes = await _dbContext.ProjectTypes.ToListAsync();
            projectTypes.Should().HaveCount(3);
        }

        [Fact]
        public async Task SeedUsersAndRolesAsync_ShouldAddPostTypes_WhenNoneExist()
        {
            // Act
            await Seed.SeedUsersAndRolesAsync(_applicationBuilder);

            // Assert
            var postTypes = await _dbContext.PostTypes.ToListAsync();
            postTypes.Should().HaveCount(3);
        }

        [Fact]
        public async Task SeedUsersAndRolesAsync_ShouldAddProjectStatuses_WhenNoneExist()
        {
            // Act
            await Seed.SeedUsersAndRolesAsync(_applicationBuilder);

            // Assert
            var projectStatuses = await _dbContext.ProjectStatuses.ToListAsync();
            projectStatuses.Should().HaveCount(4);
        }

        [Fact]
        public async Task SeedUsersAndRolesAsync_ShouldAddStudentGroups_WhenNoneExist()
        {
            // Act
            await Seed.SeedUsersAndRolesAsync(_applicationBuilder);

            // Assert
            var studentGroups = await _dbContext.StudentGroups.ToListAsync();
            studentGroups.Should().HaveCount(3);
        }
    }

}
