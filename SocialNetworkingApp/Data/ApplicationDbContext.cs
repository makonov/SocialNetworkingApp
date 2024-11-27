using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Image)
                .WithMany()
                .HasForeignKey(p => p.ImageId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Album)
                .WithMany()
                .HasForeignKey(i => i.ImageAlbumId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        public DbSet<User> Users {  get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ImageAlbum> ImageAlbums { get; set;}
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
    }
}
