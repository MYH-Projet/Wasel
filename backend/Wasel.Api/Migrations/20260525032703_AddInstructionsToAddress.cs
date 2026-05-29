using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructionsToAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "addresses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "addresses");
        }
    }
}
