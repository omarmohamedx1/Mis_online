using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CoreHrDocumentsAndDelegations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "EmployeeDocuments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "EmployeeDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "EmployeeDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentTypeId",
                table: "EmployeeDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                table: "EmployeeDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EmployeeDocuments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "IssueDate",
                table: "EmployeeDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "EmployeeDocuments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sha256Hash",
                table: "EmployeeDocuments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "EmployeeDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "EmployeeDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegationTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    AuthorizedEntity = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDelegations", x => x.Id);
                    table.CheckConstraint("CK_EmployeeDelegations_CancelState", "(\"Status\" <> 'Cancelled') OR (\"CancelledAt\" IS NOT NULL AND \"CancelledByUserId\" IS NOT NULL AND \"CancellationReason\" IS NOT NULL)");
                    table.CheckConstraint("CK_EmployeeDelegations_DateRange", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_EmployeeDelegations_Status", "\"Status\" IN ('Draft', 'Active', 'Expired', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_EmployeeDelegations_DelegationTypes_DelegationTypeId",
                        column: x => x.DelegationTypeId,
                        principalTable: "DelegationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDelegations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDelegations_Users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDelegations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDelegations_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_DeletedByUserId",
                table: "EmployeeDocuments",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_DocumentTypeId",
                table: "EmployeeDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_EmployeeId_ExpiryDate",
                table: "EmployeeDocuments",
                columns: new[] { "EmployeeId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_UpdatedByUserId",
                table: "EmployeeDocuments",
                column: "UpdatedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeDocuments_DateRange",
                table: "EmployeeDocuments",
                sql: "\"IssueDate\" IS NULL OR \"ExpiryDate\" IS NULL OR \"ExpiryDate\" >= \"IssueDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeDocuments_DeleteState",
                table: "EmployeeDocuments",
                sql: "NOT \"IsDeleted\" OR (\"DeletedAt\" IS NOT NULL AND \"DeletedByUserId\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeDocuments_FileSize",
                table: "EmployeeDocuments",
                sql: "\"FileSize\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_CancelledByUserId",
                table: "EmployeeDelegations",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_CreatedByUserId",
                table: "EmployeeDelegations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_DelegationNumber",
                table: "EmployeeDelegations",
                column: "DelegationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_DelegationTypeId",
                table: "EmployeeDelegations",
                column: "DelegationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_EmployeeId_StartDate",
                table: "EmployeeDelegations",
                columns: new[] { "EmployeeId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_Status_EndDate",
                table: "EmployeeDelegations",
                columns: new[] { "Status", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_UpdatedByUserId",
                table: "EmployeeDelegations",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDocuments_DocumentTypes_DocumentTypeId",
                table: "EmployeeDocuments",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDocuments_Users_DeletedByUserId",
                table: "EmployeeDocuments",
                column: "DeletedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDocuments_Users_UpdatedByUserId",
                table: "EmployeeDocuments",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocuments_DocumentTypes_DocumentTypeId",
                table: "EmployeeDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocuments_Users_DeletedByUserId",
                table: "EmployeeDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocuments_Users_UpdatedByUserId",
                table: "EmployeeDocuments");

            migrationBuilder.DropTable(
                name: "EmployeeDelegations");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDocuments_DeletedByUserId",
                table: "EmployeeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDocuments_DocumentTypeId",
                table: "EmployeeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDocuments_EmployeeId_ExpiryDate",
                table: "EmployeeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDocuments_UpdatedByUserId",
                table: "EmployeeDocuments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeDocuments_DateRange",
                table: "EmployeeDocuments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeDocuments_DeleteState",
                table: "EmployeeDocuments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeDocuments_FileSize",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "Sha256Hash",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "EmployeeDocuments");
        }
    }
}
