using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAbsencesV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeAbsences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AbsenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttendanceSource = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAbsences", x => x.Id);
                    table.CheckConstraint("CK_EmployeeAbsences_AttendanceSource", "\"AttendanceSource\" = 'Manual'");
                    table.CheckConstraint("CK_EmployeeAbsences_Status", "\"Status\" IN ('Pending', 'Excused', 'Unexcused')");
                    table.CheckConstraint("CK_EmployeeAbsences_Type", "\"Type\" = 'Absent'");
                    table.ForeignKey(
                        name: "FK_EmployeeAbsences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_AbsenceDate",
                table: "EmployeeAbsences",
                column: "AbsenceDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_EmployeeId_AbsenceDate",
                table: "EmployeeAbsences",
                columns: new[] { "EmployeeId", "AbsenceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_Status",
                table: "EmployeeAbsences",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAbsences");
        }
    }
}
