using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class GifInPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gif",
                table: "Posts");

            migrationBuilder.AddColumn<int>(
                name: "GifId",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_GifId",
                table: "Posts",
                column: "GifId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Gifs_GifId",
                table: "Posts",
                column: "GifId",
                principalTable: "Gifs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Gifs_GifId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_GifId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "GifId",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "Gif",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
