using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StrengthenCollectionsIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionCustomers_NationalId",
                table: "CollectionCustomers");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCustomers_OrganizationId_NationalId",
                table: "CollectionCustomers",
                columns: new[] { "OrganizationId", "NationalId" },
                unique: true,
                filter: "\"NationalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionCustomers_OrganizationId_NationalId",
                table: "CollectionCustomers");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCustomers_NationalId",
                table: "CollectionCustomers",
                column: "NationalId");
        }
    }
}
