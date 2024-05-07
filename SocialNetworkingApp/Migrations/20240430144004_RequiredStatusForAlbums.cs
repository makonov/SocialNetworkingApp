using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class RequiredStatusForAlbums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isRequired",
                table: "GifAlbums",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isRequired",
                table: "GifAlbums");
        }
    }
}
