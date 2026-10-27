using System;
using Microsoft.EntityFrameworkCore.Migrations;

// Purpose: Evolves the SQL schema for advertisement performance events and privacy-minimal device metadata.
// Responsibility: Adds only database structures required by the documented admin operations features.
#nullable disable

namespace Vivoice.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationsManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrowserFamily",
                table: "VisitorSessions",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "VisitorSessions",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AdvertisementEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvertisementId = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SessionHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertisementEvents_Advertisements_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalTable: "Advertisements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementEvents_AdvertisementId_OccurredAtUtc",
                table: "AdvertisementEvents",
                columns: new[] { "AdvertisementId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementEvents_EventType_OccurredAtUtc",
                table: "AdvertisementEvents",
                columns: new[] { "EventType", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvertisementEvents");

            migrationBuilder.DropColumn(
                name: "BrowserFamily",
                table: "VisitorSessions");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "VisitorSessions");
        }
    }
}
