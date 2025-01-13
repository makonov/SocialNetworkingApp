using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInProjectScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges");

            migrationBuilder.AddColumn<int>(
                name: "LastProjectChangeId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "ProjectFolloweres",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CommunityId",
                table: "ImageAlbums",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ImageAlbums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageAlbums_CommunityId",
                table: "ImageAlbums",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAlbums_ProjectId",
                table: "ImageAlbums",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageAlbums_Communities_CommunityId",
                table: "ImageAlbums",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageAlbums_Projects_ProjectId",
                table: "ImageAlbums",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageAlbums_Communities_CommunityId",
                table: "ImageAlbums");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageAlbums_Projects_ProjectId",
                table: "ImageAlbums");

            migrationBuilder.DropIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges");

            migrationBuilder.DropIndex(
                name: "IX_ImageAlbums_CommunityId",
                table: "ImageAlbums");

            migrationBuilder.DropIndex(
                name: "IX_ImageAlbums_ProjectId",
                table: "ImageAlbums");

            migrationBuilder.DropColumn(
                name: "LastProjectChangeId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ProjectFolloweres");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "ImageAlbums");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ImageAlbums");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges",
                column: "ProjectId");
        }
    }
}
