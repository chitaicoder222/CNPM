
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourGuide.Api.Migrations
{
    public partial class CreateTranslationJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(
                            "MySql:ValueGenerationStrategy",
                            MySqlValueGenerationStrategy.IdentityColumn),

                    Code = table.Column<string>(
                        type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    Name = table.Column<string>(
                        type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    TtsVoice = table.Column<string>(
                        type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_languages_Code",
                table: "languages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateTable(
                name: "translation_jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "MySql:ValueGenerationStrategy",
                            MySqlValueGenerationStrategy.IdentityColumn),

                    PlaceId = table.Column<long>(type: "bigint", nullable: false),

                    TargetLanguage = table.Column<string>(
                        type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    SourceText = table.Column<string>(
                        type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    SourceHash = table.Column<string>(
                        type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    Type = table.Column<string>(
                        type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    Status = table.Column<string>(
                        type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    RetryCount = table.Column<int>(
                        type: "int", nullable: false),

                    RunAfter = table.Column<DateTime>(
                        type: "datetime(6)", nullable: false),

                    StartedAt = table.Column<DateTime>(
                        type: "datetime(6)", nullable: true),

                    LastError = table.Column<string>(
                        type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    Result = table.Column<string>(
                        type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    AudioPath = table.Column<string>(
                        type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation_jobs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_translation_jobs_PlaceId_TargetLanguage_SourceHash_Type",
                table: "translation_jobs",
                columns: new[]
                {
                    "PlaceId",
                    "TargetLanguage",
                    "SourceHash",
                    "Type"
                });

            migrationBuilder.CreateIndex(
                name: "IX_translation_jobs_Status_RunAfter",
                table: "translation_jobs",
                columns: new[]
                {
                    "Status",
                    "RunAfter"
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "translation_jobs");

            migrationBuilder.DropTable(
                name: "languages");
        }
    }
}
