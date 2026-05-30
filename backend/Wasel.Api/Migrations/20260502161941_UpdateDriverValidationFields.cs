using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDriverValidationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_drivers_LicenseNumber",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "drivers");

            migrationBuilder.RenameColumn(
                name: "VehicleType",
                table: "drivers",
                newName: "PermitNumber");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "drivers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_drivers_PermitNumber",
                table: "drivers",
                column: "PermitNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drivers_UserId",
                table: "drivers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_drivers_users_UserId",
                table: "drivers",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_drivers_users_UserId",
                table: "drivers");

            migrationBuilder.DropIndex(
                name: "IX_drivers_PermitNumber",
                table: "drivers");

            migrationBuilder.DropIndex(
                name: "IX_drivers_UserId",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "drivers");

            migrationBuilder.RenameColumn(
                name: "PermitNumber",
                table: "drivers",
                newName: "VehicleType");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "drivers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "drivers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_drivers_LicenseNumber",
                table: "drivers",
                column: "LicenseNumber",
                unique: true);
        }
    }
}
