using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "users",
                newName: "Phone");

            migrationBuilder.AddColumn<string>(
                name: "Cin",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeycloakId",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileObjectKey",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "KeycloakId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ProfileObjectKey",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "users",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
