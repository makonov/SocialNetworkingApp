using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    public partial class userIdFromIntToStringInFriendRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_FromUserId1",
                table: "FriendRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ToUserId1",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_FromUserId1",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_ToUserId1",
                table: "FriendRequests");

            migrationBuilder.DropColumn(
                name: "FromUserId1",
                table: "FriendRequests");

            migrationBuilder.DropColumn(
                name: "ToUserId1",
                table: "FriendRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ToUserId",
                table: "FriendRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FromUserId",
                table: "FriendRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_FromUserId",
                table: "FriendRequests",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_ToUserId",
                table: "FriendRequests",
                column: "ToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_FromUserId",
                table: "FriendRequests",
                column: "FromUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ToUserId",
                table: "FriendRequests",
                column: "ToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_FromUserId",
                table: "FriendRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ToUserId",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_FromUserId",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_ToUserId",
                table: "FriendRequests");

            migrationBuilder.AlterColumn<int>(
                name: "ToUserId",
                table: "FriendRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FromUserId",
                table: "FriendRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromUserId1",
                table: "FriendRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToUserId1",
                table: "FriendRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_FromUserId1",
                table: "FriendRequests",
                column: "FromUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_ToUserId1",
                table: "FriendRequests",
                column: "ToUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_FromUserId1",
                table: "FriendRequests",
                column: "FromUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ToUserId1",
                table: "FriendRequests",
                column: "ToUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
