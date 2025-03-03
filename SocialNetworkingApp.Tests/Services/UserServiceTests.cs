using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Services;
using SocialNetworkingApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Services
{
    public class UserServiceTests
    {
        private UserManager<User> _userManager;
        private UserService _userService;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _context;

        public UserServiceTests()
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

            var userStore = new UserStore<User>(_context);
            var userValidator = new UserValidator<User>();
            var passwordHasher = new PasswordHasher<User>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>().Object;
            var logger = new Mock<ILogger<UserManager<User>>>().Object;

            _userManager = new UserManager<User>(
                userStore,
                optionsAccessor,
                passwordHasher,
                new List<IUserValidator<User>> { userValidator },
                new List<IPasswordValidator<User>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object,
                logger
            );

            var roleLogger = new Mock<ILogger<RoleManager<IdentityRole>>>().Object;
            var roleStore = new RoleStore<IdentityRole>(_context);
            _roleManager = new RoleManager<IdentityRole>(
                roleStore,
                new[] { new RoleValidator<IdentityRole>() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                roleLogger 
            );

            _userService = new UserService(_userManager);  
        }

        [Fact]
        public async Task UserServiceTests_FindUsersPagedAsync_ShouldReturnFilteredUsers()
        {
            // Arrange
            await SetUpAsync();

            var viewModel = new FindFriendViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male",
                FromAge = 20,
                ToAge = 30
            };

            var currentUserId = "currentUserId";
            var pageNumber = 1;
            var pageSize = 10;

            var users = new List<User>
            {
                new User { FirstName = "John", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-25), IsMale = true, UserName = "username1" },
                new User { FirstName = "Jane", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-22), IsMale = false, UserName = "username2" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            // Назначаем роли пользователям
            foreach (var user in users)
            {
                var r = await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            // Act
            var result = await _userService.FindUsersPagedAsync(viewModel, currentUserId, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().FirstName.Should().Be("John");
            result.First().LastName.Should().Be("Doe");
        }


        [Fact]
        public async Task UserServiceTests_FindUsersPagedAsync_ShouldFilterByGenderFemale()
        {
            // Arrange
            await SetUpAsync();

            var viewModel = new FindFriendViewModel
            {
                Gender = "Female",
                LastName = "Doe", 
                FromAge = 20,
                ToAge = 30
            };

            var currentUserId = "currentUserId";
            var pageNumber = 1;
            var pageSize = 10;

            var users = new List<User>
            {
                new User { FirstName = "John", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-25), IsMale = true, UserName = "username1" },
                new User { FirstName = "Jane", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-22), IsMale = false, UserName = "username2" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            foreach (var user in users)
            {
                var r = await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            // Act
            var result = await _userService.FindUsersPagedAsync(viewModel, currentUserId, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);  
            result.First().FirstName.Should().Be("Jane");
            result.First().LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task UserServiceTests_FindUsersPagedAsync_ShouldThrowArgumentException_WhenFromAgeGreaterThanToAge()
        {
            // Arrange
            await SetUpAsync();

            var viewModel = new FindFriendViewModel
            {
                FromAge = 30, 
                ToAge = 20    
            };

            var currentUserId = "currentUserId";
            var pageNumber = 1;
            var pageSize = 10;

            var users = new List<User>
            {
                new User { FirstName = "John", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-25), IsMale = true, UserName = "username1" },
                new User { FirstName = "Jane", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-22), IsMale = false, UserName = "username2" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            foreach (var user in users)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            // Act & Assert
            Func<Task> act = async () => await _userService.FindUsersPagedAsync(viewModel, currentUserId, pageNumber, pageSize);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Нижняя граница возраста не может быть больше верхней границы");
        }

        [Fact]
        public async Task UserServiceTests_FindUsersPagedAsync_ShouldFilterByStudentGroupId()
        {
            // Arrange
            await SetUpAsync();

            var viewModel = new FindFriendViewModel
            {
                StudentGroupId = 1  
            };

            var currentUserId = "currentUserId";
            var pageNumber = 1;
            var pageSize = 10;

            var users = new List<User>
            {
                new User { FirstName = "John", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-25), IsMale = true, UserName = "username1", GroupId = 1 },
                new User { FirstName = "Jane", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-22), IsMale = false, UserName = "username2", GroupId = 2 }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            foreach (var user in users)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            // Act
            var result = await _userService.FindUsersPagedAsync(viewModel, currentUserId, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);  
            result.First().FirstName.Should().Be("John");
            result.First().LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task UserServiceTests_GetAllUsersExceptCurrentUserAsync_ShouldReturnAllUsersExceptCurrentUser()
        {
            // Arrange
            await SetUpAsync();

            var currentUserId = "currentUserId"; 
            var users = new List<User>
            {
                new User { Id = "user1", FirstName = "John", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-25), IsMale = true, UserName = "username1" },
                new User { Id = "user2", FirstName = "Jane", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-22), IsMale = false, UserName = "username2" },
                new User { Id = "currentUserId", FirstName = "Current", LastName = "User", BirthDate = DateTime.Now.AddYears(-28), IsMale = true, UserName = "username3" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            foreach (var user in users)
            {
                if (user.Id != currentUserId)
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.User);
                }
            }

            // Act
            var result = await _userService.GetAllUsersExceptCurrentUserAsync(currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);  
            result.Should().Contain(u => u.Id == "user1");
            result.Should().Contain(u => u.Id == "user2");
            result.Should().NotContain(u => u.Id == currentUserId); 
        }

        [Fact]
        public async Task UserServiceTests_GetCurrentUserAsync_ShouldReturnCurrentUser()
        {
            // Arrange
            await SetUpAsync();
            
            var user = new User { FirstName = "Current", LastName = "User", BirthDate = DateTime.Now.AddYears(-28), IsMale = true, UserName = "username3" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var currentUserId = user.Id;

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            await _userManager.AddToRoleAsync(user, UserRoles.User);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, currentUserId),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var identity = new ClaimsIdentity(claims, "mock");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _userService.GetCurrentUserAsync(principal);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(user); 
        }

        [Fact]
        public async Task UserServiceTests_GetUserByIdAsync_ShouldReturnUserWithGroup()
        {
            // Arrange
            await SetUpAsync();

            var userId = "userId1"; 
            var group = new StudentGroup { GroupName = "Test Group" };
            _context.StudentGroups.Add(group);
            await _context.SaveChangesAsync();


            var user = new User
            {
                Id = userId,
                FirstName = "Test",
                LastName = "User",
                BirthDate = DateTime.Now.AddYears(-28),
                IsMale = true,
                UserName = "username4",
                GroupId = group.Id,  
                Group = group         
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            await _userManager.AddToRoleAsync(user, UserRoles.User);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(user);  
            result.Group.Should().NotBeNull(); 
            result.Group.GroupName.Should().Be("Test Group");  
        }

        [Fact]
        public async Task UserServiceTests_GetUserByUserNameAsync_ShouldReturnUserWithGroup()
        {
            // Arrange
            await SetUpAsync();

            var userName = "username5";  

            var group = new StudentGroup { GroupName = "Test Group" };
            _context.StudentGroups.Add(group);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Id = "userId2",
                FirstName = "John",
                LastName = "Doe",
                BirthDate = DateTime.Now.AddYears(-25),
                IsMale = true,
                UserName = userName,  
                GroupId = group.Id,   
                Group = group         
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            await _userManager.AddToRoleAsync(user, UserRoles.User);

            // Act
            var result = await _userService.GetUserByUserNameAsync(userName);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be(userName); 
            result.Group.Should().NotBeNull();  
            result.Group.GroupName.Should().Be("Test Group");  
        }

        [Fact]
        public async Task UserServiceTests_GetPagedUsers_ShouldReturnPagedUsers()
        {
            // Arrange
            await SetUpAsync();

            var currentUserId = "currentUserId"; 
            var page = 1; 
            var pageSize = 2;  

            var users = new List<User>
            {
                new User { Id = "userId1", FirstName = "John", LastName = "Doe", BirthDate = DateTime.Now.AddYears(-25), IsMale = true, UserName = "username1" },
                new User { Id = "userId2", FirstName = "Jane", LastName = "Smith", BirthDate = DateTime.Now.AddYears(-22), IsMale = false, UserName = "username2" },
                new User { Id = "userId3", FirstName = "Alice", LastName = "Johnson", BirthDate = DateTime.Now.AddYears(-23), IsMale = false, UserName = "username3" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            foreach (var user in users)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            // Act
            var result = await _userService.GetPagedUsers(currentUserId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(pageSize);  
            result.Should().NotContain(u => u.Id == currentUserId);
            result[0].LastName.Should().Be("Doe");
            result[1].LastName.Should().Be("Johnson");
            result.All(u => _userManager.GetRolesAsync(u).Result.Contains(UserRoles.User)).Should().BeTrue();
        }

        [Fact]
        public async Task UserServiceTests_SearchUsersAsync_ShouldReturnMatchingUsers()
        {
            // Arrange
            await SetUpAsync();

            var currentUserId = "currentUserId";
            var querySingle = "John"; 
            var queryFull = "Jane Smith"; 

            var users = new List<User>
            {
                new User { Id = "userId1", FirstName = "John", LastName = "Doe", UserName = "john_doe" },
                new User { Id = "userId2", FirstName = "Jane", LastName = "Smith", UserName = "jane_smith" },
                new User { Id = "userId3", FirstName = "Alice", LastName = "Johnson", UserName = "alice_johnson" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            foreach (var user in users)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            // Act
            var resultSingle = await _userService.SearchUsersAsync(querySingle, currentUserId);
            var resultFull = await _userService.SearchUsersAsync(queryFull, currentUserId);

            // Assert
            resultSingle.Should().NotBeNull();
            resultSingle.Should().ContainSingle(u => u.FirstName == "John" && u.LastName == "Doe");

            resultFull.Should().NotBeNull();
            resultFull.Should().ContainSingle(u => u.FirstName == "Jane" && u.LastName == "Smith");

            resultSingle.Should().NotContain(u => u.Id == currentUserId);
            resultFull.Should().NotContain(u => u.Id == currentUserId);
            resultSingle.All(u => _userManager.GetRolesAsync(u).Result.Contains(UserRoles.User)).Should().BeTrue();
            resultFull.All(u => _userManager.GetRolesAsync(u).Result.Contains(UserRoles.User)).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")] 
        public async Task UserServiceTests_SearchUsersAsync_ShouldReturnEmptyList_WhenQueryIsNullOrWhiteSpace(string query)
        {
            // Arrange
            await SetUpAsync();
            var currentUserId = "currentUserId";

            // Act
            var result = await _userService.SearchUsersAsync(query, currentUserId);

            // Assert
            result.Should().NotBeNull(); 
            result.Should().BeEmpty(); 
        }

        [Fact]
        public async Task UserServiceTests_GetSelectListOfUsers_ShouldReturnUsersInRole()
        {
            // Arrange
            await SetUpAsync();

            var users = new List<User>
            {
                new User { Id = "1", FirstName = "John", LastName = "Doe", UserName = "john_doe" },
                new User { Id = "2", FirstName = "Jane", LastName = "Smith", UserName = "jane_smith" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            foreach (var user in users)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            // Act
            var result = await _userService.GetSelectListOfUsers();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Value == "1" && u.Text == "John Doe");
            result.Should().Contain(u => u.Value == "2" && u.Text == "Jane Smith");
        }

        [Fact]
        public async Task UserServiceTests_SearchUsersAsync_ShouldReturnFilteredUsers()
        {
            // Arrange
            await SetUpAsync();

            var users = new List<User>
            {
                new User { Id = "1", FirstName = "John", LastName = "Doe", UserName = "john_doe", GroupId = 1, BirthDate = new DateTime(2000, 1, 1) },
                new User { Id = "2", FirstName = "Jane", LastName = "Smith", UserName = "jane_smith", GroupId = 2, BirthDate = new DateTime(1995, 5, 5) },
                new User { Id = "3", FirstName = "Jack", LastName = "Brown", UserName = "jack_brown", GroupId = 1, BirthDate = new DateTime(2000, 1, 1) }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync(UserRoles.User)) 
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

            await _userManager.AddToRoleAsync(users[0], UserRoles.User);
            await _userManager.AddToRoleAsync(users[1], UserRoles.Admin);
            await _userManager.AddToRoleAsync(users[2], UserRoles.User);

            var filter = new FilterUsersViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                GroupId = 1,
                UserRole = UserRoles.User,
                BirthDate = new DateTime(2000, 1, 1)
            };

            // Act
            var result = await _userService.SearchUsersAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().FirstName.Should().Be("John");
            result.First().LastName.Should().Be("Doe");
            result.First().GroupId.Should().Be(1);
            result.First().BirthDate.Should().Be(new DateTime(2000, 1, 1));
        }



    }




}
