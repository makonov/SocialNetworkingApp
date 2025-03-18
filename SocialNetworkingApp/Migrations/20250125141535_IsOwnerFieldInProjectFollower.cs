using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace SocialNetworkingApp.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class IsOwnerFieldInProjectFollower : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "ProjectFolloweres",
                newName: "IsOwner");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsOwner",
                table: "ProjectFolloweres",
                newName: "IsAdmin");
        }
    }
}
