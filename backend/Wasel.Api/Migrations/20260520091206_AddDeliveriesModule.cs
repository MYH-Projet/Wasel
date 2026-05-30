using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveriesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryTime",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "PickupAddress",
                table: "deliveries");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "deliveries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "DropoffAddressId",
                table: "deliveries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ParcelId",
                table: "deliveries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "deliveries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PickupAddressId",
                table: "deliveries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_addresses_users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delivery_status_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_status_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_delivery_status_histories_deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parcels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsFragile = table.Column<bool>(type: "boolean", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deliveries_ClientId",
                table: "deliveries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_deliveries_DropoffAddressId",
                table: "deliveries",
                column: "DropoffAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_deliveries_ParcelId",
                table: "deliveries",
                column: "ParcelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deliveries_PickupAddressId",
                table: "deliveries",
                column: "PickupAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_addresses_ClientId",
                table: "addresses",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_status_histories_DeliveryId",
                table: "delivery_status_histories",
                column: "DeliveryId");

            migrationBuilder.AddForeignKey(
                name: "FK_deliveries_addresses_DropoffAddressId",
                table: "deliveries",
                column: "DropoffAddressId",
                principalTable: "addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_deliveries_addresses_PickupAddressId",
                table: "deliveries",
                column: "PickupAddressId",
                principalTable: "addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_deliveries_parcels_ParcelId",
                table: "deliveries",
                column: "ParcelId",
                principalTable: "parcels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_deliveries_users_ClientId",
                table: "deliveries",
                column: "ClientId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deliveries_addresses_DropoffAddressId",
                table: "deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_deliveries_addresses_PickupAddressId",
                table: "deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_deliveries_parcels_ParcelId",
                table: "deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_deliveries_users_ClientId",
                table: "deliveries");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "delivery_status_histories");

            migrationBuilder.DropTable(
                name: "parcels");

            migrationBuilder.DropIndex(
                name: "IX_deliveries_ClientId",
                table: "deliveries");

            migrationBuilder.DropIndex(
                name: "IX_deliveries_DropoffAddressId",
                table: "deliveries");

            migrationBuilder.DropIndex(
                name: "IX_deliveries_ParcelId",
                table: "deliveries");

            migrationBuilder.DropIndex(
                name: "IX_deliveries_PickupAddressId",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "DropoffAddressId",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "ParcelId",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "PickupAddressId",
                table: "deliveries");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "deliveries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "deliveries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DriverId",
                table: "deliveries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryTime",
                table: "deliveries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress",
                table: "deliveries",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
