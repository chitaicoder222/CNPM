using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourGuide.Api.Migrations
{
    public partial class FixMissingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create languages table if it does not exist (original migration may have never run)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `languages` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `Code` varchar(12) NOT NULL,
                    `Name` longtext NOT NULL,
                    `TtsVoice` longtext NOT NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_languages_Code` (`Code`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ");

            // Create places table (minimal) for foreign key usage
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `places` (
                    `Id` bigint NOT NULL AUTO_INCREMENT,
                    PRIMARY KEY (`Id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `places`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `languages`;");
        }
    }
}
