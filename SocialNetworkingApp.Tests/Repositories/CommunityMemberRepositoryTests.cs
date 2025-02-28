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
    public class CommunityMemberRepositoryTests
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
        public async Task CommunityMemberRepository_Add_Should_Save_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var member = new CommunityMember { UserId = "user1", CommunityId = 1 };

            // Act
            repository.Add(member);
            await dbContext.SaveChangesAsync();

            // Assert
            var savedMember = await dbContext.CommunityMembers.FindAsync(member.Id);
            savedMember.Should().NotBeNull();
            savedMember!.UserId.Should().Be("user1");
            savedMember.CommunityId.Should().Be(1);
        }

        [Fact]
        public async Task CommunityMemberRepository_Delete_Should_Remove_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var member = new CommunityMember { UserId = "user2", CommunityId = 2 };

            repository.Add(member);
            await dbContext.SaveChangesAsync();

            // Act
            repository.Delete(member);
            await dbContext.SaveChangesAsync();

            // Assert
            var deletedMember = await dbContext.CommunityMembers.FindAsync(member.Id);
            deletedMember.Should().BeNull();
        }

        [Fact]
        public async Task CommunityMemberRepository_GetAllByUserIdAsync_Should_Return_Correct_Users()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var userId = "user3";

            var user = new User { Id = userId, UserName = "TestUser3" };
            await dbContext.Users.AddAsync(user);

            var communities = new List<Community>
            {
                new Community { Id = 1, Title = "Community 1" },
                new Community { Id = 2, Title = "Community 2" }
            };
            await dbContext.Communities.AddRangeAsync(communities);

            await dbContext.SaveChangesAsync();

            var members = new List<CommunityMember>
            {
                new CommunityMember { UserId = userId, CommunityId = 1 },
                new CommunityMember { UserId = userId, CommunityId = 2 }
            };

            await dbContext.CommunityMembers.AddRangeAsync(members);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.All(m => m.UserId == userId).Should().BeTrue();
        }


        [Fact]
        public async Task CommunityMemberRepository_GetByCommunityIdAsync_Should_Return_Correct_Members()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var communityId = 3;

            var users = new List<User>
            {
                new User { Id = "user4", UserName = "User 4" },
                new User { Id = "user5", UserName = "User 5" }
            };
            await dbContext.Users.AddRangeAsync(users);

            var community = new Community { Id = communityId, Title = "Community 3" };
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            var members = new List<CommunityMember>
            {
                new CommunityMember { UserId = "user4", CommunityId = communityId },
                new CommunityMember { UserId = "user5", CommunityId = communityId }
            };

            await dbContext.CommunityMembers.AddRangeAsync(members);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByCommunityIdAsync(communityId);

            // Assert
            result.Should().HaveCount(2);
            result.All(m => m.CommunityId == communityId).Should().BeTrue();
        }


        [Fact]
        public async Task CommunityMemberRepository_IsMember_Should_Return_True_If_User_Is_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var userId = "user6";
            var communityId = 4;

            var user = new User { Id = userId, UserName = "User 6" };
            var community = new Community { Id = communityId, Title = "Community 4" };

            await dbContext.Users.AddAsync(user);
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            var member = new CommunityMember { UserId = userId, CommunityId = communityId };
            await dbContext.CommunityMembers.AddAsync(member);
            await dbContext.SaveChangesAsync();

            // Act
            var isMember = await repository.IsMember(userId, communityId);

            // Assert
            isMember.Should().BeTrue();
        }


        [Fact]
        public async Task CommunityMemberRepository_IsAdmin_Should_Return_True_If_User_Is_Admin()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var userId = "adminUser";
            var communityId = 5;

            var user = new User { Id = userId, UserName = "Admin User" };
            var community = new Community { Id = communityId, Title = "Community 5" };

            await dbContext.Users.AddAsync(user);
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            var adminMember = new CommunityMember { UserId = userId, CommunityId = communityId, IsAdmin = true };
            await dbContext.CommunityMembers.AddAsync(adminMember);
            await dbContext.SaveChangesAsync();

            // Act
            var isAdmin = await repository.IsAdmin(userId, communityId);

            // Assert
            isAdmin.Should().BeTrue();
        }


        [Fact]
        public async Task CommunityMemberRepository_GetAdminsByCommunityIdAsync_Should_Return_Admins()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var communityId = 6;

            var users = new List<User>
            {
                new User { Id = "admin1", UserName = "Admin 1" },
                new User { Id = "admin2", UserName = "Admin 2" },
                new User { Id = "user7", UserName = "User 7" }
            };
            await dbContext.Users.AddRangeAsync(users);

            var community = new Community { Id = communityId, Title = "Community 6" };
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            var members = new List<CommunityMember>
            {
                new CommunityMember { UserId = "admin1", CommunityId = communityId, IsAdmin = true },
                new CommunityMember { UserId = "admin2", CommunityId = communityId, IsAdmin = true },
                new CommunityMember { UserId = "user7", CommunityId = communityId, IsAdmin = false }
            };

            await dbContext.CommunityMembers.AddRangeAsync(members);
            await dbContext.SaveChangesAsync();

            // Act
            var admins = await repository.GetAdminsByCommunityIdAsync(communityId);

            // Assert
            admins.Should().HaveCount(2);
            admins.All(m => m.IsAdmin).Should().BeTrue();
        }

        [Fact]
        public async Task CommunityMemberRepository_GetByIdAsync_Should_Return_Correct_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var communityId = 7;
            var userId = "user8";

            var user = new User { Id = userId, UserName = "User 8" };
            var community = new Community { Id = communityId, Title = "Community 7" };

            await dbContext.Users.AddAsync(user);
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            var member = new CommunityMember { UserId = userId, CommunityId = communityId };
            await dbContext.CommunityMembers.AddAsync(member);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(member.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(member.Id);
            result.UserId.Should().Be(userId);
            result.CommunityId.Should().Be(communityId);
        }

        [Fact]
        public async Task CommunityMemberRepository_GetByUserIdAndCommunityIdAsync_Should_Return_Correct_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var communityId = 8;
            var userId = "user9";

            var user = new User { Id = userId, UserName = "User 9" };
            var community = new Community { Id = communityId, Title = "Community 8" };

            await dbContext.Users.AddAsync(user);
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            var member = new CommunityMember { UserId = userId, CommunityId = communityId };
            await dbContext.CommunityMembers.AddAsync(member);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserIdAndCommunityIdAsync(userId, communityId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.CommunityId.Should().Be(communityId);
        }

        [Fact]
        public async Task CommunityMemberRepository_Update_Should_Modify_Existing_Member()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);
            var communityId = 9;
            var userId = "user10";

            var user = new User { Id = userId, UserName = "User 10" };
            var community = new Community { Id = communityId, Title = "Community 9" };

            await dbContext.Users.AddAsync(user);
            await dbContext.Communities.AddAsync(community);
            await dbContext.SaveChangesAsync();

            var member = new CommunityMember { UserId = userId, CommunityId = communityId, IsAdmin = false };
            await dbContext.CommunityMembers.AddAsync(member);
            await dbContext.SaveChangesAsync();

            member.IsAdmin = true;

            // Act
            var updated = repository.Update(member);
            await dbContext.SaveChangesAsync();

            var updatedMember = await repository.GetByIdAsync(member.Id);

            // Assert
            updated.Should().BeTrue();
            updatedMember.Should().NotBeNull();
            updatedMember!.IsAdmin.Should().BeTrue();
        }

        [Fact]
        public async Task CommunityMemberRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new CommunityMemberRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

    }
}
