using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionsSecureAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionAttachments_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAttachments_CollectionPayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "CollectionPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAttachments_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAttachments_CaseId_UploadedAt",
                table: "CollectionAttachments",
                columns: new[] { "CaseId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAttachments_FileHash_CaseId",
                table: "CollectionAttachments",
                columns: new[] { "FileHash", "CaseId" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAttachments_PaymentId",
                table: "CollectionAttachments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAttachments_UploadedById",
                table: "CollectionAttachments",
                column: "UploadedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionAttachments");
        }
    }
}
