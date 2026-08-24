using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MIS.Infrastructure.Persistence;

#nullable disable
namespace MIS.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260814060000_LinkCollectionCasesToBankPortfolioImports")]
public sealed class LinkCollectionCasesToBankPortfolioImports : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "SourceImportId", table: "CollectionCases", type: "uuid", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_CollectionCases_SourceImportId", table: "CollectionCases", column: "SourceImportId");
        migrationBuilder.AddForeignKey(name: "FK_CollectionCases_BankPortfolioImports_SourceImportId", table: "CollectionCases",
            column: "SourceImportId", principalTable: "BankPortfolioImports", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_CollectionCases_BankPortfolioImports_SourceImportId", table: "CollectionCases");
        migrationBuilder.DropIndex(name: "IX_CollectionCases_SourceImportId", table: "CollectionCases");
        migrationBuilder.DropColumn(name: "SourceImportId", table: "CollectionCases");
    }
}
