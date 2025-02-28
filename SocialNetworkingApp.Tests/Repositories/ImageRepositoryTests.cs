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
    public class ImageRepositoryTests
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
        public async Task ImageRepository_Add_Should_Add_Image()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            // Создание альбома для изображения
            var album = new ImageAlbum { Name = "Album 1", UserId = "user1" };
            dbContext.ImageAlbums.Add(album);
            await dbContext.SaveChangesAsync();

            var image = new Image
            {
                ImagePath = "path/to/image1",
                ImageAlbumId = album.Id
            };

            // Act
            var result = repository.Add(image);

            // Assert
            result.Should().BeTrue();
            var addedImage = await dbContext.Images.FirstOrDefaultAsync(i => i.ImagePath == image.ImagePath);
            addedImage.Should().NotBeNull(); 
            addedImage.ImagePath.Should().Be(image.ImagePath); 
        }

        [Fact]
        public async Task ImageRepository_Delete_Should_Remove_Image()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            var album = new ImageAlbum { Name = "Album 2", UserId = "user2" };
            dbContext.ImageAlbums.Add(album);
            await dbContext.SaveChangesAsync();

            var image = new Image
            {
                ImagePath = "path/to/image2",
                ImageAlbumId = album.Id
            };
            dbContext.Images.Add(image);
            await dbContext.SaveChangesAsync();

            // Act
            var result = repository.Delete(image);

            // Assert
            result.Should().BeTrue(); 
            var deletedImage = await dbContext.Images.FirstOrDefaultAsync(i => i.ImagePath == image.ImagePath);
            deletedImage.Should().BeNull(); 
        }

        [Fact]
        public async Task ImageRepository_GetByIdAsync_Should_Return_Image()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            var album = new ImageAlbum { Name = "Album 3", UserId = "user3" };
            dbContext.ImageAlbums.Add(album);
            await dbContext.SaveChangesAsync();

            var image = new Image
            {
                ImagePath = "path/to/image3",
                ImageAlbumId = album.Id
            };
            dbContext.Images.Add(image);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(image.Id);

            // Assert
            result.Should().NotBeNull(); 
            result.ImagePath.Should().Be(image.ImagePath);
        }

        [Fact]
        public async Task ImageRepository_GetAllAsync_Should_Return_All_Images()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            var album1 = new ImageAlbum { Name = "Album 4", UserId = "user4" };
            var album2 = new ImageAlbum { Name = "Album 5", UserId = "user5" };
            dbContext.ImageAlbums.AddRange(album1, album2);
            await dbContext.SaveChangesAsync();

            var image1 = new Image { ImagePath = "path/to/image1", ImageAlbumId = album1.Id };
            var image2 = new Image { ImagePath = "path/to/image2", ImageAlbumId = album2.Id };
            dbContext.Images.AddRange(image1, image2);
            await dbContext.SaveChangesAsync();

            // Act
            var images = await repository.GetAllAsync();

            // Assert
            images.Should().HaveCount(2); 
        }

        [Fact]
        public async Task ImageRepository_GetByAlbumIdAsync_Should_Return_Images_For_Album()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            var album = new ImageAlbum { Name = "Album 6", UserId = "user6" };
            dbContext.ImageAlbums.Add(album);
            await dbContext.SaveChangesAsync();

            var image1 = new Image { ImagePath = "path/to/image1", ImageAlbumId = album.Id };
            var image2 = new Image { ImagePath = "path/to/image2", ImageAlbumId = album.Id };
            var image3 = new Image { ImagePath = "path/to/image3", ImageAlbumId = 999 }; 
            dbContext.Images.AddRange(image1, image2, image3);
            await dbContext.SaveChangesAsync();

            // Act
            var images = await repository.GetByAlbumIdAsync(album.Id);

            // Assert
            images.Should().HaveCount(2); 
            images.Should().Contain(i => i.ImagePath == "path/to/image1");
            images.Should().Contain(i => i.ImagePath == "path/to/image2");
            images.Should().NotContain(i => i.ImagePath == "path/to/image3");
        }

        [Fact]
        public async Task ImageRepository_GetByPathAsync_Should_Return_Image_By_Path()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            // Создание альбома для изображения
            var album = new ImageAlbum { Name = "Album 7", UserId = "user7" };
            dbContext.ImageAlbums.Add(album);
            await dbContext.SaveChangesAsync();

            var image = new Image
            {
                ImagePath = "path/to/image4",
                ImageAlbumId = album.Id
            };
            dbContext.Images.Add(image);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByPathAsync("path/to/image4");

            // Assert
            result.Should().NotBeNull(); 
            result.ImagePath.Should().Be(image.ImagePath); 
        }

        [Fact]
        public void ImageRepository_Save_Should_Return_False_If_No_Changes_Are_Saved()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            // Act
            var result = repository.Save();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ImageRepository_Update_Should_Modify_Existing_Image()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ImageRepository(dbContext);

            var album = new ImageAlbum { Name = "Album 8", UserId = "user8" };
            dbContext.ImageAlbums.Add(album);
            dbContext.SaveChanges();

            var image = new Image
            {
                ImagePath = "path/to/image5",
                ImageAlbumId = album.Id
            };
            dbContext.Images.Add(image);
            dbContext.SaveChanges();

            // Act
            image.ImagePath = "path/to/updated_image";
            var result = repository.Update(image);

            // Assert
            result.Should().BeTrue();
            var updatedImage = dbContext.Images.FirstOrDefault(i => i.ImagePath == "path/to/updated_image");
            updatedImage.Should().NotBeNull();
            updatedImage.ImagePath.Should().Be("path/to/updated_image");
        }

    }
}
