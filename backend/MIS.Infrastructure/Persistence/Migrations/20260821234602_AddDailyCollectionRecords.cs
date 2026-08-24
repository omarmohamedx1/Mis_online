using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MIS.Infrastructure.Persistence;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260821234602_AddDailyCollectionRecords")]
public partial class AddDailyCollectionRecords : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CollectionDcrs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BankId = table.Column<Guid>(type: "uuid", nullable: false),
                CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                DcrDate = table.Column<DateOnly>(type: "date", nullable: false),
                ActionCover = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                PtpDate = table.Column<DateOnly>(type: "date", nullable: true),
                PtpAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                PaidDate = table.Column<DateOnly>(type: "date", nullable: true),
                PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                FollowUpAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                VisitDate = table.Column<DateOnly>(type: "date", nullable: true),
                LinkedPtpId = table.Column<Guid>(type: "uuid", nullable: true),
                LinkedVisitId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CollectionDcrs", x => x.Id);
                table.CheckConstraint("CK_CollectionDcrs_Amounts", "(\"PtpAmount\" IS NULL OR \"PtpAmount\" > 0) AND (\"PaidAmount\" IS NULL OR \"PaidAmount\" > 0)");
                table.ForeignKey("FK_CollectionDcrs_CollectionCases_CaseId", x => x.CaseId, "CollectionCases", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_CollectionDcrs_CollectionClientOrganizations_BankId", x => x.BankId, "CollectionClientOrganizations", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_CollectionDcrs_CollectionFieldVisits_LinkedVisitId", x => x.LinkedVisitId, "CollectionFieldVisits", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_CollectionDcrs_CollectionPromisesToPay_LinkedPtpId", x => x.LinkedPtpId, "CollectionPromisesToPay", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_CollectionDcrs_Users_CreatedByUserId", x => x.CreatedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_CollectionDcrs_BankId_DcrDate", "CollectionDcrs", new[] { "BankId", "DcrDate" });
        migrationBuilder.CreateIndex("IX_CollectionDcrs_CaseId_CreatedAt", "CollectionDcrs", new[] { "CaseId", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_CollectionDcrs_CreatedByUserId_DcrDate", "CollectionDcrs", new[] { "CreatedByUserId", "DcrDate" });
        migrationBuilder.CreateIndex("IX_CollectionDcrs_LinkedPtpId", "CollectionDcrs", "LinkedPtpId");
        migrationBuilder.CreateIndex("IX_CollectionDcrs_LinkedVisitId", "CollectionDcrs", "LinkedVisitId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("CollectionDcrs");
}
