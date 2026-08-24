using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankArchiveLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchiveNotes",
                table: "CollectionCases",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                table: "CollectionCases",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "CollectionCases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedById",
                table: "CollectionCases",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "CollectionCases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RestoreReason",
                table: "CollectionCases",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RestoredAt",
                table: "CollectionCases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestoredById",
                table: "CollectionCases",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchiveNotes",
                table: "BankPortfolioImports",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                table: "BankPortfolioImports",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "BankPortfolioImports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedById",
                table: "BankPortfolioImports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "BankPortfolioImports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RestoreReason",
                table: "BankPortfolioImports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RestoredAt",
                table: "BankPortfolioImports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestoredById",
                table: "BankPortfolioImports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_ArchivedAt",
                table: "CollectionCases",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_ArchivedById",
                table: "CollectionCases",
                column: "ArchivedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_PortfolioId_IsArchived",
                table: "CollectionCases",
                columns: new[] { "PortfolioId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_RestoredById",
                table: "CollectionCases",
                column: "RestoredById");

            migrationBuilder.CreateIndex(
                name: "IX_BankPortfolioImports_ArchivedAt",
                table: "BankPortfolioImports",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BankPortfolioImports_ArchivedById",
                table: "BankPortfolioImports",
                column: "ArchivedById");

            migrationBuilder.CreateIndex(
                name: "IX_BankPortfolioImports_BankId_IsArchived",
                table: "BankPortfolioImports",
                columns: new[] { "BankId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_BankPortfolioImports_RestoredById",
                table: "BankPortfolioImports",
                column: "RestoredById");

            migrationBuilder.AddForeignKey(
                name: "FK_BankPortfolioImports_Users_ArchivedById",
                table: "BankPortfolioImports",
                column: "ArchivedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankPortfolioImports_Users_RestoredById",
                table: "BankPortfolioImports",
                column: "RestoredById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCases_Users_ArchivedById",
                table: "CollectionCases",
                column: "ArchivedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCases_Users_RestoredById",
                table: "CollectionCases",
                column: "RestoredById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankPortfolioImports_Users_ArchivedById",
                table: "BankPortfolioImports");

            migrationBuilder.DropForeignKey(
                name: "FK_BankPortfolioImports_Users_RestoredById",
                table: "BankPortfolioImports");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCases_Users_ArchivedById",
                table: "CollectionCases");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCases_Users_RestoredById",
                table: "CollectionCases");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCases_ArchivedAt",
                table: "CollectionCases");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCases_ArchivedById",
                table: "CollectionCases");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCases_PortfolioId_IsArchived",
                table: "CollectionCases");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCases_RestoredById",
                table: "CollectionCases");

            migrationBuilder.DropIndex(
                name: "IX_BankPortfolioImports_ArchivedAt",
                table: "BankPortfolioImports");

            migrationBuilder.DropIndex(
                name: "IX_BankPortfolioImports_ArchivedById",
                table: "BankPortfolioImports");

            migrationBuilder.DropIndex(
                name: "IX_BankPortfolioImports_BankId_IsArchived",
                table: "BankPortfolioImports");

            migrationBuilder.DropIndex(
                name: "IX_BankPortfolioImports_RestoredById",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "ArchiveNotes",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "ArchivedById",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "RestoreReason",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "RestoredAt",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "RestoredById",
                table: "CollectionCases");

            migrationBuilder.DropColumn(
                name: "ArchiveNotes",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "ArchivedById",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "RestoreReason",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "RestoredAt",
                table: "BankPortfolioImports");

            migrationBuilder.DropColumn(
                name: "RestoredById",
                table: "BankPortfolioImports");
        }
    }
}
