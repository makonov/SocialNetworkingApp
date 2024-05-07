using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class SetNullBehaviourForGifInAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GifAlbums_Gifs_GifId",
                table: "GifAlbums");

            migrationBuilder.DropIndex(
                name: "IX_GifAlbums_GifId",
                table: "GifAlbums");

            migrationBuilder.AddColumn<int>(
                name: "AlbumId",
                table: "Gifs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gifs_AlbumId",
                table: "Gifs",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_GifAlbums_GifId",
                table: "GifAlbums",
                column: "GifId");

            migrationBuilder.AddForeignKey(
                name: "FK_GifAlbums_Gifs_GifId",
                table: "GifAlbums",
                column: "GifId",
                principalTable: "Gifs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Gifs_GifAlbums_AlbumId",
                table: "Gifs",
                column: "AlbumId",
                principalTable: "GifAlbums",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GifAlbums_Gifs_GifId",
                table: "GifAlbums");

            migrationBuilder.DropForeignKey(
                name: "FK_Gifs_GifAlbums_AlbumId",
                table: "Gifs");

            migrationBuilder.DropIndex(
                name: "IX_Gifs_AlbumId",
                table: "Gifs");

            migrationBuilder.DropIndex(
                name: "IX_GifAlbums_GifId",
                table: "GifAlbums");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "Gifs");

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
    }
}
