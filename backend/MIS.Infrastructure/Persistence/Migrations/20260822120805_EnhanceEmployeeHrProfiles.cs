using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceEmployeeHrProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                table: "Employees",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedByUserId",
                table: "Employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FingerprintEnrollmentDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperationalRole",
                table: "Employees",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ArchivedByUserId",
                table: "Employees",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsArchived_Status",
                table: "Employees",
                columns: new[] { "IsArchived", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_OperationalRole",
                table: "Employees",
                column: "OperationalRole");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_ArchivedByUserId",
                table: "Employees",
                column: "ArchivedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_ArchivedByUserId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ArchivedByUserId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IsArchived_Status",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_OperationalRole",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "FingerprintEnrollmentDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OperationalRole",
                table: "Employees");
        }
    }
}
