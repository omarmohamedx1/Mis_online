using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankVisitsManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "CollectionFieldVisits",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CollectionFieldVisits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"CollectionFieldVisits\" SET \"UpdatedAt\" = COALESCE(\"CheckedOutAt\", \"CreatedAt\") WHERE \"UpdatedAt\" IS NULL;");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CollectionFieldVisits",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFieldVisits_Status_ScheduledAt",
                table: "CollectionFieldVisits",
                columns: new[] { "Status", "ScheduledAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionFieldVisits_Status_ScheduledAt",
                table: "CollectionFieldVisits");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "CollectionFieldVisits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CollectionFieldVisits");
        }
    }
}
