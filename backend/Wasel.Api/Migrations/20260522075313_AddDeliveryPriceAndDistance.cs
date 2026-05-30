using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryPriceAndDistance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistanceKm",
                table: "deliveries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "deliveries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "deliveries");
        }
    }
}
