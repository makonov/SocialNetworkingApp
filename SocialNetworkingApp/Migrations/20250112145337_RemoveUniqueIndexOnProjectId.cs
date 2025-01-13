using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueIndexOnProjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Удаление уникального индекса (если он существует)
            migrationBuilder.DropIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges");

            // Создаем обычный индекс без уникальности
            migrationBuilder.CreateIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Восстанавливаем уникальный индекс (если откатываем миграцию)
            migrationBuilder.DropIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectChanges_ProjectId",
                table: "ProjectChanges",
                column: "ProjectId",
                unique: true);
        }
    }
}
