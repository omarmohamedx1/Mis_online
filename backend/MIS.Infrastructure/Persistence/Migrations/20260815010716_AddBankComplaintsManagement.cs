using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankComplaintsManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SlaDueAt",
                table: "CollectionComplaints",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "CollectionComplaints",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "CollectionComplaints",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolvedAt",
                table: "CollectionComplaints",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedById",
                table: "CollectionComplaints",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CollectionComplaints",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"CollectionComplaints\" SET \"UpdatedAt\" = \"ReceivedAt\" WHERE \"UpdatedAt\" IS NULL;");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CollectionComplaints",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ComplaintId",
                table: "CollectionAttachments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CollectionComplaintNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionComplaintNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionComplaintNotes_CollectionComplaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "CollectionComplaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionComplaintNotes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaints_CaseId_ReceivedAt",
                table: "CollectionComplaints",
                columns: new[] { "CaseId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaints_ResolvedById",
                table: "CollectionComplaints",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAttachments_ComplaintId_UploadedAt",
                table: "CollectionAttachments",
                columns: new[] { "ComplaintId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaintNotes_ComplaintId_CreatedAt",
                table: "CollectionComplaintNotes",
                columns: new[] { "ComplaintId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaintNotes_CreatedById",
                table: "CollectionComplaintNotes",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionAttachments_CollectionComplaints_ComplaintId",
                table: "CollectionAttachments",
                column: "ComplaintId",
                principalTable: "CollectionComplaints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionComplaints_Users_ResolvedById",
                table: "CollectionComplaints",
                column: "ResolvedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionAttachments_CollectionComplaints_ComplaintId",
                table: "CollectionAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionComplaints_Users_ResolvedById",
                table: "CollectionComplaints");

            migrationBuilder.DropTable(
                name: "CollectionComplaintNotes");

            migrationBuilder.DropIndex(
                name: "IX_CollectionComplaints_CaseId_ReceivedAt",
                table: "CollectionComplaints");

            migrationBuilder.DropIndex(
                name: "IX_CollectionComplaints_ResolvedById",
                table: "CollectionComplaints");

            migrationBuilder.DropIndex(
                name: "IX_CollectionAttachments_ComplaintId_UploadedAt",
                table: "CollectionAttachments");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "CollectionComplaints");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "CollectionComplaints");

            migrationBuilder.DropColumn(
                name: "ResolvedById",
                table: "CollectionComplaints");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CollectionComplaints");

            migrationBuilder.DropColumn(
                name: "ComplaintId",
                table: "CollectionAttachments");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SlaDueAt",
                table: "CollectionComplaints",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "CollectionComplaints",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
