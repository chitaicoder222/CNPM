using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vivoice.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforcePoiGpsAndReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementEvents_Advertisements_AdvertisementId",
                table: "AdvertisementEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Sponsors_SponsorId",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AdminUsers_AdminUserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PoiTopics_Pois_PoiId",
                table: "PoiTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_PoiTranslations_Pois_PoiId",
                table: "PoiTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_SponsorshipAgreements_Sponsors_SponsorId",
                table: "SponsorshipAgreements");

            migrationBuilder.DropForeignKey(
                name: "FK_SponsorshipAgreementVisitRates_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipAgreementVisitRates");

            migrationBuilder.DropForeignKey(
                name: "FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicTranslations_PoiTopics_PoiTopicId",
                table: "TopicTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_VisitRevenueRates_VisitRevenueRateId",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSponsorshipRevenues_Visits_VisitId",
                table: "VisitSponsorshipRevenues");

            // The product request replaces the legacy POI catalog. Children are deleted first so
            // the subsequent Restrict foreign keys never allow an implicit cascade delete.
            migrationBuilder.Sql("DELETE FROM [TopicTranslations]; DELETE FROM [PoiTranslations]; DELETE FROM [PoiTopics]; DELETE FROM [Pois];");

            migrationBuilder.AddColumn<int>(
                name: "TriggerRadiusMeters",
                table: "Pois",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Pois_TriggerRadiusMeters_Positive",
                table: "Pois",
                sql: "[TriggerRadiusMeters] > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementEvents_Advertisements_AdvertisementId",
                table: "AdvertisementEvents",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Sponsors_SponsorId",
                table: "Advertisements",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AdminUsers_AdminUserId",
                table: "AuditLogs",
                column: "AdminUserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoiTopics_Pois_PoiId",
                table: "PoiTopics",
                column: "PoiId",
                principalTable: "Pois",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoiTranslations_Pois_PoiId",
                table: "PoiTranslations",
                column: "PoiId",
                principalTable: "Pois",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipAgreements_Sponsors_SponsorId",
                table: "SponsorshipAgreements",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipAgreementVisitRates_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipAgreementVisitRates",
                column: "SponsorshipAgreementId",
                principalTable: "SponsorshipAgreements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipPayments",
                column: "SponsorshipAgreementId",
                principalTable: "SponsorshipAgreements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicTranslations_PoiTopics_PoiTopicId",
                table: "TopicTranslations",
                column: "PoiTopicId",
                principalTable: "PoiTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_VisitRevenueRates_VisitRevenueRateId",
                table: "Visits",
                column: "VisitRevenueRateId",
                principalTable: "VisitRevenueRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits",
                column: "VisitorSessionId",
                principalTable: "VisitorSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSponsorshipRevenues_Visits_VisitId",
                table: "VisitSponsorshipRevenues",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementEvents_Advertisements_AdvertisementId",
                table: "AdvertisementEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Sponsors_SponsorId",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AdminUsers_AdminUserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PoiTopics_Pois_PoiId",
                table: "PoiTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_PoiTranslations_Pois_PoiId",
                table: "PoiTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_SponsorshipAgreements_Sponsors_SponsorId",
                table: "SponsorshipAgreements");

            migrationBuilder.DropForeignKey(
                name: "FK_SponsorshipAgreementVisitRates_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipAgreementVisitRates");

            migrationBuilder.DropForeignKey(
                name: "FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicTranslations_PoiTopics_PoiTopicId",
                table: "TopicTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_VisitRevenueRates_VisitRevenueRateId",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSponsorshipRevenues_Visits_VisitId",
                table: "VisitSponsorshipRevenues");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Pois_TriggerRadiusMeters_Positive",
                table: "Pois");

            migrationBuilder.DropColumn(
                name: "TriggerRadiusMeters",
                table: "Pois");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementEvents_Advertisements_AdvertisementId",
                table: "AdvertisementEvents",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Sponsors_SponsorId",
                table: "Advertisements",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AdminUsers_AdminUserId",
                table: "AuditLogs",
                column: "AdminUserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoiTopics_Pois_PoiId",
                table: "PoiTopics",
                column: "PoiId",
                principalTable: "Pois",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoiTranslations_Pois_PoiId",
                table: "PoiTranslations",
                column: "PoiId",
                principalTable: "Pois",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipAgreements_Sponsors_SponsorId",
                table: "SponsorshipAgreements",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipAgreementVisitRates_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipAgreementVisitRates",
                column: "SponsorshipAgreementId",
                principalTable: "SponsorshipAgreements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipPayments",
                column: "SponsorshipAgreementId",
                principalTable: "SponsorshipAgreements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicTranslations_PoiTopics_PoiTopicId",
                table: "TopicTranslations",
                column: "PoiTopicId",
                principalTable: "PoiTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_VisitRevenueRates_VisitRevenueRateId",
                table: "Visits",
                column: "VisitRevenueRateId",
                principalTable: "VisitRevenueRates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits",
                column: "VisitorSessionId",
                principalTable: "VisitorSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSponsorshipRevenues_Visits_VisitId",
                table: "VisitSponsorshipRevenues",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
