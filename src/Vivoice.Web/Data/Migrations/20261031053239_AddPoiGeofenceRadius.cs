using Microsoft.EntityFrameworkCore.Migrations;

// Purpose: Adds a positive GPS narration radius and preserves existing POIs with a 100-meter default.
#nullable disable

namespace Vivoice.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPoiGeofenceRadius : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RadiusMeters",
                table: "Pois",
                type: "float",
                nullable: false,
                defaultValue: 100.0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Pois_RadiusMeters_Positive",
                table: "Pois",
                sql: "[RadiusMeters] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Pois_RadiusMeters_Positive",
                table: "Pois");

            migrationBuilder.DropColumn(
                name: "RadiusMeters",
                table: "Pois");
        }
    }
}
