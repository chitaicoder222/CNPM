using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourGuide.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceGeoMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "places",
                type: "varchar(240)",
                maxLength: 240,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "places",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "places",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "places",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "places",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "places",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "places");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "places");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "places");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "places");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "places");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "places");
        }
    }
}
