using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vivoice.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorCampaignVisitPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingMode",
                table: "SponsorshipAgreements",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "fixed");

            migrationBuilder.AddColumn<string>(
                name: "CampaignName",
                table: "SponsorshipAgreements",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "Legacy agreement");

            migrationBuilder.CreateTable(
                name: "SponsorshipAgreementVisitRates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SponsorshipAgreementId = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipAgreementVisitRates", x => x.Id);
                    table.UniqueConstraint("AK_SponsorshipAgreementVisitRates_Id_SponsorshipAgreementId", x => new { x.Id, x.SponsorshipAgreementId });
                    table.ForeignKey(
                        name: "FK_SponsorshipAgreementVisitRates_SponsorshipAgreements_SponsorshipAgreementId",
                        column: x => x.SponsorshipAgreementId,
                        principalTable: "SponsorshipAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitSponsorshipRevenues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<long>(type: "bigint", nullable: false),
                    SponsorshipAgreementId = table.Column<long>(type: "bigint", nullable: false),
                    SponsorshipAgreementVisitRateId = table.Column<long>(type: "bigint", nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitSponsorshipRevenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitSponsorshipRevenues_SponsorshipAgreementVisitRates_SponsorshipAgreementVisitRateId_SponsorshipAgreementId",
                        columns: x => new { x.SponsorshipAgreementVisitRateId, x.SponsorshipAgreementId },
                        principalTable: "SponsorshipAgreementVisitRates",
                        principalColumns: new[] { "Id", "SponsorshipAgreementId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisitSponsorshipRevenues_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipAgreementVisitRates_SponsorshipAgreementId_EffectiveFromUtc",
                table: "SponsorshipAgreementVisitRates",
                columns: new[] { "SponsorshipAgreementId", "EffectiveFromUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitSponsorshipRevenues_SponsorshipAgreementId",
                table: "VisitSponsorshipRevenues",
                column: "SponsorshipAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSponsorshipRevenues_SponsorshipAgreementVisitRateId_SponsorshipAgreementId",
                table: "VisitSponsorshipRevenues",
                columns: new[] { "SponsorshipAgreementVisitRateId", "SponsorshipAgreementId" });

            migrationBuilder.CreateIndex(
                name: "IX_VisitSponsorshipRevenues_VisitId_SponsorshipAgreementId",
                table: "VisitSponsorshipRevenues",
                columns: new[] { "VisitId", "SponsorshipAgreementId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitSponsorshipRevenues");

            migrationBuilder.DropTable(
                name: "SponsorshipAgreementVisitRates");

            migrationBuilder.DropColumn(
                name: "BillingMode",
                table: "SponsorshipAgreements");

            migrationBuilder.DropColumn(
                name: "CampaignName",
                table: "SponsorshipAgreements");
        }
    }
}
