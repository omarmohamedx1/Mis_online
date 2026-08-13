using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImmutableUserLoginCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoginCode",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "LoginCode" = 'USR-' || UPPER(SUBSTRING(REPLACE("Id"::text, '-', '') FROM 1 FOR 8));
                """);

            migrationBuilder.AlterColumn<string>(
                name: "LoginCode",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_LoginCode",
                table: "Users",
                column: "LoginCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_LoginCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginCode",
                table: "Users");
        }
    }
}
