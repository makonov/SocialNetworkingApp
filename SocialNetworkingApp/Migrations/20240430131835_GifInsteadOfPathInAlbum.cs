using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class GifInsteadOfPathInAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gifs_GifAlbums_GifAlbumId",
                table: "Gifs");

            migrationBuilder.DropIndex(
                name: "IX_Gifs_GifAlbumId",
                table: "Gifs");

            migrationBuilder.DropColumn(
                name: "GifPath",
                table: "GifAlbums");

            migrationBuilder.AddColumn<int>(
                name: "GifId",
                table: "GifAlbums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GifAlbums_GifId",
                table: "GifAlbums",
                column: "GifId",
                unique: true,
                filter: "[GifId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_GifAlbums_Gifs_GifId",
                table: "GifAlbums",
                column: "GifId",
                principalTable: "Gifs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GifAlbums_Gifs_GifId",
                table: "GifAlbums");

            migrationBuilder.DropIndex(
                name: "IX_GifAlbums_GifId",
                table: "GifAlbums");

            migrationBuilder.DropColumn(
                name: "GifId",
                table: "GifAlbums");

            migrationBuilder.AddColumn<string>(
                name: "GifPath",
                table: "GifAlbums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gifs_GifAlbumId",
                table: "Gifs",
                column: "GifAlbumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gifs_GifAlbums_GifAlbumId",
                table: "Gifs",
                column: "GifAlbumId",
                principalTable: "GifAlbums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
