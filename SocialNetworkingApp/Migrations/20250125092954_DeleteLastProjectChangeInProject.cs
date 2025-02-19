using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class DeleteLastProjectChangeInProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges");

            migrationBuilder.DropColumn(
                name: "LastProjectChangeId",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges");

            migrationBuilder.AddColumn<int>(
                name: "LastProjectChangeId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges",
                column: "ProjectId",
                unique: true);
        }
    }
}
