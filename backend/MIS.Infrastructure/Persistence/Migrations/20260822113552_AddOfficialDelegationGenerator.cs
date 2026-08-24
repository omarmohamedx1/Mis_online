using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialDelegationGenerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "EmployeeDelegations",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "CompanyRepresentative",
                table: "EmployeeDelegations",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DelegatingEntityId",
                table: "EmployeeDelegations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNameSnapshot",
                table: "EmployeeDelegations",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNationalIdSnapshot",
                table: "EmployeeDelegations",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumberSnapshot",
                table: "EmployeeDelegations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowerOfAttorneyNumber",
                table: "EmployeeDelegations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PowerOfAttorneyYear",
                table: "EmployeeDelegations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDelegations_DelegatingEntityId",
                table: "EmployeeDelegations",
                column: "DelegatingEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDelegations_CollectionClientOrganizations_Delegatin~",
                table: "EmployeeDelegations",
                column: "DelegatingEntityId",
                principalTable: "CollectionClientOrganizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDelegations_CollectionClientOrganizations_Delegatin~",
                table: "EmployeeDelegations");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDelegations_DelegatingEntityId",
                table: "EmployeeDelegations");

            migrationBuilder.DropColumn(
                name: "CompanyRepresentative",
                table: "EmployeeDelegations");

            migrationBuilder.DropColumn(
                name: "DelegatingEntityId",
                table: "EmployeeDelegations");

            migrationBuilder.DropColumn(
                name: "EmployeeNameSnapshot",
                table: "EmployeeDelegations");

            migrationBuilder.DropColumn(
                name: "EmployeeNationalIdSnapshot",
                table: "EmployeeDelegations");

            migrationBuilder.DropColumn(
                name: "EmployeeNumberSnapshot",
                table: "EmployeeDelegations");

            migrationBuilder.DropColumn(
                name: "PowerOfAttorneyNumber",
                table: "EmployeeDelegations");

            migrationBuilder.DropColumn(
                name: "PowerOfAttorneyYear",
                table: "EmployeeDelegations");

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "EmployeeDelegations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);
        }
    }
}
