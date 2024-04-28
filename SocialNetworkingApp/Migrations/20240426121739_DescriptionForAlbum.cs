using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionForAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GifAlbums",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "GifAlbums");
        }
    }
}
