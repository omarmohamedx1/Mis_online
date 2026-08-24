using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StrengthenHrOperationalIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "EmployeeAbsences"
                        GROUP BY "EmployeeId", "AbsenceDate"
                        HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Duplicate employee/date absence records must be resolved before applying StrengthenHrOperationalIntegrity.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAbsences_EmployeeId_AbsenceDate",
                table: "EmployeeAbsences");

            // Some development databases received this replacement index before the
            // migration-history row was recorded. Remove that orphaned copy so the
            // migration remains recoverable and can recreate the intended index.
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_EmployeeAbsences_Employee_Date\";");

            migrationBuilder.CreateIndex(
                name: "UX_EmployeeAbsences_Employee_Date",
                table: "EmployeeAbsences",
                columns: new[] { "EmployeeId", "AbsenceDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_EmployeeAbsences_Employee_Date",
                table: "EmployeeAbsences");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAbsences_EmployeeId_AbsenceDate",
                table: "EmployeeAbsences",
                columns: new[] { "EmployeeId", "AbsenceDate" });
        }
    }
}
