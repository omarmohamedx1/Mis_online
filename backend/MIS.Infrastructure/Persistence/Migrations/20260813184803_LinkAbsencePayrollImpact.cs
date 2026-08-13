using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkAbsencePayrollImpact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedDeductionAmount",
                table: "EmployeeAbsences",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayrollImpactStatus",
                table: "EmployeeAbsences",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "NotApplicable");

            migrationBuilder.AddColumn<string>(
                name: "PayrollNotes",
                table: "EmployeeAbsences",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PayrollReviewedAt",
                table: "EmployeeAbsences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayrollReviewedByUserId",
                table: "EmployeeAbsences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SuggestedDeductionAmount",
                table: "EmployeeAbsences",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_PayrollImpactStatus",
                table: "EmployeeAbsences",
                column: "PayrollImpactStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_PayrollReviewedByUserId",
                table: "EmployeeAbsences",
                column: "PayrollReviewedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeAbsences_DeductionAmounts",
                table: "EmployeeAbsences",
                sql: "\"SuggestedDeductionAmount\" >= 0 AND (\"ApprovedDeductionAmount\" IS NULL OR \"ApprovedDeductionAmount\" >= 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeAbsences_PayrollImpactStatus",
                table: "EmployeeAbsences",
                sql: "\"PayrollImpactStatus\" IN ('NotApplicable', 'PendingReview', 'Approved', 'Excluded')");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAbsences_Users_PayrollReviewedByUserId",
                table: "EmployeeAbsences",
                column: "PayrollReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                UPDATE "EmployeeAbsences" AS absence
                SET "PayrollImpactStatus" = 'PendingReview',
                    "SuggestedDeductionAmount" = COALESCE((
                        SELECT ROUND(compensation."BasicSalary" / 30.0, 2)
                        FROM "EmployeeCompensations" AS compensation
                        WHERE compensation."EmployeeId" = absence."EmployeeId"
                          AND compensation."EffectiveFrom" <= absence."AbsenceDate"
                          AND (compensation."EffectiveTo" IS NULL OR compensation."EffectiveTo" >= absence."AbsenceDate")
                        ORDER BY compensation."EffectiveFrom" DESC
                        LIMIT 1
                    ), 0)
                WHERE absence."Status" = 'Unexcused';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAbsences_Users_PayrollReviewedByUserId",
                table: "EmployeeAbsences");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAbsences_PayrollImpactStatus",
                table: "EmployeeAbsences");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAbsences_PayrollReviewedByUserId",
                table: "EmployeeAbsences");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeAbsences_DeductionAmounts",
                table: "EmployeeAbsences");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeAbsences_PayrollImpactStatus",
                table: "EmployeeAbsences");

            migrationBuilder.DropColumn(
                name: "ApprovedDeductionAmount",
                table: "EmployeeAbsences");

            migrationBuilder.DropColumn(
                name: "PayrollImpactStatus",
                table: "EmployeeAbsences");

            migrationBuilder.DropColumn(
                name: "PayrollNotes",
                table: "EmployeeAbsences");

            migrationBuilder.DropColumn(
                name: "PayrollReviewedAt",
                table: "EmployeeAbsences");

            migrationBuilder.DropColumn(
                name: "PayrollReviewedByUserId",
                table: "EmployeeAbsences");

            migrationBuilder.DropColumn(
                name: "SuggestedDeductionAmount",
                table: "EmployeeAbsences");
        }
    }
}
