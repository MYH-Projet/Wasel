using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wasel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintEvidences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectKey = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintEvidences_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_DeliveryId",
                table: "Complaints",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintEvidences_ComplaintId",
                table: "ComplaintEvidences",
                column: "ComplaintId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_deliveries_DeliveryId",
                table: "Complaints",
                column: "DeliveryId",
                principalTable: "deliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_deliveries_DeliveryId",
                table: "Complaints");

            migrationBuilder.DropTable(
                name: "ComplaintEvidences");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_DeliveryId",
                table: "Complaints");
        }
    }
}
