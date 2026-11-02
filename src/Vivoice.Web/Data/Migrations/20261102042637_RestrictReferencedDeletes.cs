using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vivoice.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestrictReferencedDeletes : Migration
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
                name: "FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicTranslations_PoiTopics_PoiTopicId",
                table: "TopicTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementEvents_Advertisements_AdvertisementId",
                table: "AdvertisementEvents",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Sponsors_SponsorId",
                table: "Advertisements",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AdminUsers_AdminUserId",
                table: "AuditLogs",
                column: "AdminUserId",
                principalTable: "AdminUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PoiTopics_Pois_PoiId",
                table: "PoiTopics",
                column: "PoiId",
                principalTable: "Pois",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PoiTranslations_Pois_PoiId",
                table: "PoiTranslations",
                column: "PoiId",
                principalTable: "Pois",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipAgreements_Sponsors_SponsorId",
                table: "SponsorshipAgreements",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipPayments",
                column: "SponsorshipAgreementId",
                principalTable: "SponsorshipAgreements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TopicTranslations_PoiTopics_PoiTopicId",
                table: "TopicTranslations",
                column: "PoiTopicId",
                principalTable: "PoiTopics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits",
                column: "VisitorSessionId",
                principalTable: "VisitorSessions",
                principalColumn: "Id");
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
                name: "FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId",
                table: "SponsorshipPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicTranslations_PoiTopics_PoiTopicId",
                table: "TopicTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits");

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
                name: "FK_Visits_VisitorSessions_VisitorSessionId",
                table: "Visits",
                column: "VisitorSessionId",
                principalTable: "VisitorSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
