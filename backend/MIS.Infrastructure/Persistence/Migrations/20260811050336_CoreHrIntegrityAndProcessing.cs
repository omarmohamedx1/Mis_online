using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CoreHrIntegrityAndProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT LOWER(TRIM("EmployeeNumber"))
                        FROM "Employees"
                        GROUP BY LOWER(TRIM("EmployeeNumber"))
                        HAVING COUNT(*) > 1)
                    THEN
                        RAISE EXCEPTION 'Employees contain case-insensitive duplicate employee IDs. Resolve them before applying this migration.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "LeaveRequests" first_request
                        JOIN "LeaveRequests" second_request
                          ON first_request."EmployeeId" = second_request."EmployeeId"
                         AND first_request."Id" < second_request."Id"
                         AND first_request."Status" IN ('Pending', 'Approved')
                         AND second_request."Status" IN ('Pending', 'Approved')
                         AND first_request."StartDate" <= second_request."EndDate"
                         AND first_request."EndDate" >= second_request."StartDate")
                    THEN
                        RAISE EXCEPTION 'Overlapping pending or approved leave requests exist. Resolve them before applying this migration.';
                    END IF;
                END $$;

                UPDATE "Employees" SET "EmployeeNumber" = UPPER(TRIM("EmployeeNumber"));
                CREATE UNIQUE INDEX "UX_Employees_EmployeeNumber_CI" ON "Employees" (LOWER("EmployeeNumber"));

                CREATE EXTENSION IF NOT EXISTS btree_gist;
                ALTER TABLE "LeaveRequests"
                    ADD CONSTRAINT "EX_LeaveRequests_Employee_ActiveDateRange"
                    EXCLUDE USING gist
                    ("EmployeeId" WITH =, daterange("StartDate", "EndDate", '[]') WITH &&)
                    WHERE ("Status" IN ('Pending', 'Approved'));
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_AttendanceRecords_Source",
                table: "AttendanceRecords");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AttendanceRecords_Source",
                table: "AttendanceRecords",
                sql: "\"Source\" IN ('ExcelImport', 'Manual', 'DeviceIntegration', 'SystemProcessing')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "LeaveRequests" DROP CONSTRAINT IF EXISTS "EX_LeaveRequests_Employee_ActiveDateRange";
                DROP INDEX IF EXISTS "UX_Employees_EmployeeNumber_CI";
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_AttendanceRecords_Source",
                table: "AttendanceRecords");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AttendanceRecords_Source",
                table: "AttendanceRecords",
                sql: "\"Source\" IN ('ExcelImport', 'Manual', 'DeviceIntegration')");
        }
    }
}
