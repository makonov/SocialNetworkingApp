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
    public class ImageAlbumRepositoryTests
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
        public void ImageAlbumRepository_Add_Should_Add_Album()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var album = new ImageAlbum
            {
                Name = "Test Album",
                UserId = "user1",
                ProjectId = 1,
                CommunityId = 1
            };

            // Act
            var result = repository.Add(album);

            // Assert
            result.Should().BeTrue(); 
            var addedAlbum = dbContext.ImageAlbums
                .FirstOrDefault(a => a.Name == "Test Album" && a.UserId == "user1");
            addedAlbum.Should().NotBeNull(); 
        }

        [Fact]
        public void ImageAlbumRepository_Delete_Should_Remove_Album()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var album = new ImageAlbum
            {
                Name = "Test Album",
                UserId = "user1",
                ProjectId = 1,
                CommunityId = 1
            };
            dbContext.ImageAlbums.Add(album);
            dbContext.SaveChanges();

            // Act
            var result = repository.Delete(album);

            // Assert
            result.Should().BeTrue(); 
            var deletedAlbum = dbContext.ImageAlbums
                .FirstOrDefault(a => a.Name == "Test Album" && a.UserId == "user1");
            deletedAlbum.Should().BeNull(); 
        }

        [Fact]
        public void ImageAlbumRepository_Update_Should_Modify_Album()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var album = new ImageAlbum
            {
                Name = "Test Album",
                UserId = "user1",
                ProjectId = 1,
                CommunityId = 1
            };
            dbContext.ImageAlbums.Add(album);
            dbContext.SaveChanges();

            // Act
            album.Name = "Updated Album";
            var result = repository.Update(album); 

            // Assert
            result.Should().BeTrue(); 
            var updatedAlbum = dbContext.ImageAlbums
                .FirstOrDefault(a => a.Name == "Updated Album");
            updatedAlbum.Should().NotBeNull();
            updatedAlbum.Name.Should().Be("Updated Album"); 
        }

        [Fact]
        public async Task ImageAlbumRepository_GetAllByUserAsync_Should_Return_Albums_For_User()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var user = new User { Id = "user1", UserName = "user1" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var album1 = new ImageAlbum { Name = "Album 1", UserId = "user1" };
            var album2 = new ImageAlbum { Name = "Album 2", UserId = "user1" };
            dbContext.ImageAlbums.AddRange(album1, album2);
            dbContext.SaveChanges();

            // Act
            var albums = await repository.GetAllByUserAsync("user1");

            // Assert
            albums.Should().HaveCount(2);
            albums.Should().Contain(a => a.Name == "Album 1"); 
            albums.Should().Contain(a => a.Name == "Album 2"); 
        }

        [Fact]
        public async Task ImageAlbumRepository_GetByIdAsync_Should_Return_Album_By_Id()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var album = new ImageAlbum { Name = "Test Album", UserId = "user1" };
            dbContext.ImageAlbums.Add(album);
            dbContext.SaveChanges();

            // Act
            var result = await repository.GetByIdAsync(album.Id); 

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Album");
        }

        [Fact]
        public void ImageAlbumRepository_AlbumExists_Should_Return_True_If_Album_Exists()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var album = new ImageAlbum { Name = "Test Album", UserId = "user1" };
            dbContext.ImageAlbums.Add(album);
            dbContext.SaveChanges();

            // Act
            var result = repository.AlbumExists("Test Album");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ImageAlbumRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ImageAlbumRepository_GetAllByCommunityAsync_Should_Return_Albums_For_Community()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var communityId = 1;
            var album1 = new ImageAlbum { Name = "Community Album 1", UserId = "user1", CommunityId = communityId };
            var album2 = new ImageAlbum { Name = "Community Album 2", UserId = "user2", CommunityId = communityId };
            var album3 = new ImageAlbum { Name = "Other Community Album", UserId = "user3", CommunityId = 2 };
            dbContext.ImageAlbums.AddRange(album1, album2, album3);
            dbContext.SaveChanges();

            // Act
            var albums = await repository.GetAllByCommunityAsync(communityId);

            // Assert
            albums.Should().HaveCount(2); 
            albums.Should().Contain(a => a.Name == "Community Album 1"); 
            albums.Should().Contain(a => a.Name == "Community Album 2"); 
            albums.Should().NotContain(a => a.Name == "Other Community Album");
        }

        [Fact]
        public async Task ImageAlbumRepository_GetAllByProjectAsync_Should_Return_Albums_For_Project()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageAlbumRepository(dbContext);

            var projectId = 1;
            var album1 = new ImageAlbum { Name = "Project Album 1", UserId = "user1", ProjectId = projectId };
            var album2 = new ImageAlbum { Name = "Project Album 2", UserId = "user2", ProjectId = projectId };
            var album3 = new ImageAlbum { Name = "Other Project Album", UserId = "user3", ProjectId = 2 };
            dbContext.ImageAlbums.AddRange(album1, album2, album3);
            dbContext.SaveChanges();

            // Act
            var albums = await repository.GetAllByProjectAsync(projectId);

            // Assert
            albums.Should().HaveCount(2); 
            albums.Should().Contain(a => a.Name == "Project Album 1"); 
            albums.Should().Contain(a => a.Name == "Project Album 2"); 
            albums.Should().NotContain(a => a.Name == "Other Project Album"); 
        }

    }
}
