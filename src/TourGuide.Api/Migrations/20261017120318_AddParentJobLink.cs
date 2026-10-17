using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourGuide.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddParentJobLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ParentJobId",
                table: "translation_jobs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_translation_jobs_ParentJobId",
                table: "translation_jobs",
                column: "ParentJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_translation_jobs_ParentJobId",
                table: "translation_jobs");

            migrationBuilder.DropColumn(
                name: "ParentJobId",
                table: "translation_jobs");
        }
    }
}
