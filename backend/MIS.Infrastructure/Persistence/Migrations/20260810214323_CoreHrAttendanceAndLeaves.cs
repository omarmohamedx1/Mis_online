using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CoreHrAttendanceAndLeaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(127)", maxLength: 127, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MappingJson = table.Column<string>(type: "jsonb", nullable: true),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    ValidRows = table.Column<int>(type: "integer", nullable: false),
                    InvalidRows = table.Column<int>(type: "integer", nullable: false),
                    EmployeeNotFoundRows = table.Column<int>(type: "integer", nullable: false),
                    DuplicateRows = table.Column<int>(type: "integer", nullable: false),
                    MissingCheckInRows = table.Column<int>(type: "integer", nullable: false),
                    MissingCheckOutRows = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImportedRecords = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceImportBatches", x => x.Id);
                    table.CheckConstraint("CK_AttendanceImportBatches_Counts", "\"TotalRows\" >= 0 AND \"ValidRows\" >= 0 AND \"InvalidRows\" >= 0 AND \"EmployeeNotFoundRows\" >= 0 AND \"DuplicateRows\" >= 0 AND \"MissingCheckInRows\" >= 0 AND \"MissingCheckOutRows\" >= 0 AND \"ImportedRecords\" >= 0");
                    table.CheckConstraint("CK_AttendanceImportBatches_FileSize", "\"FileSize\" > 0");
                    table.CheckConstraint("CK_AttendanceImportBatches_ImportedCount", "\"ImportedRecords\" <= \"ValidRows\"");
                    table.CheckConstraint("CK_AttendanceImportBatches_Status", "\"Status\" IN ('Uploaded', 'PreviewReady', 'Confirmed', 'Failed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_AttendanceImportBatches_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaveEntitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BaseEntitlement = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Adjustment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaveEntitlements", x => x.Id);
                    table.CheckConstraint("CK_EmployeeLeaveEntitlements_Base", "\"BaseEntitlement\" >= 0");
                    table.CheckConstraint("CK_EmployeeLeaveEntitlements_Total", "\"BaseEntitlement\" + \"Adjustment\" >= 0");
                    table.CheckConstraint("CK_EmployeeLeaveEntitlements_Year", "\"Year\" BETWEEN 1900 AND 9999");
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveEntitlements_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveEntitlements_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveEntitlements_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveEntitlements_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NumberOfDays = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttachmentDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.CheckConstraint("CK_LeaveRequests_DateRange", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_LeaveRequests_Decision", "(\"Status\" = 'Pending' AND \"DecidedByUserId\" IS NULL AND \"DecidedAt\" IS NULL) OR (\"Status\" <> 'Pending' AND \"DecidedByUserId\" IS NOT NULL AND \"DecidedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_LeaveRequests_DecisionReason", "\"Status\" NOT IN ('Rejected', 'Cancelled') OR (\"DecisionNotes\" IS NOT NULL AND length(btrim(\"DecisionNotes\")) > 0)");
                    table.CheckConstraint("CK_LeaveRequests_NumberOfDays", "\"NumberOfDays\" > 0");
                    table.CheckConstraint("CK_LeaveRequests_Status", "\"Status\" IN ('Pending', 'Approved', 'Rejected', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_EmployeeDocuments_AttachmentDocumentId",
                        column: x => x.AttachmentDocumentId,
                        principalTable: "EmployeeDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkingCalendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingCalendars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceImportRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRowNumbersJson = table.Column<string>(type: "jsonb", nullable: false),
                    SourceRowsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SourceEmployeeNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceEmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CheckIn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CheckOut = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PunchesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CategoriesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ErrorsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CanImport = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceImportRows", x => x.Id);
                    table.CheckConstraint("CK_AttendanceImportRows_CanImport", "NOT \"CanImport\" OR (\"EmployeeId\" IS NOT NULL AND \"AttendanceDate\" IS NOT NULL)");
                    table.CheckConstraint("CK_AttendanceImportRows_CheckTimes", "\"CheckOut\" IS NULL OR \"CheckIn\" IS NULL OR \"CheckOut\" >= \"CheckIn\"");
                    table.ForeignKey(
                        name: "FK_AttendanceImportRows_AttendanceImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "AttendanceImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceImportRows_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckIn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CheckOut = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WorkingMinutes = table.Column<int>(type: "integer", nullable: false),
                    LateMinutes = table.Column<int>(type: "integer", nullable: false),
                    EarlyLeaveMinutes = table.Column<int>(type: "integer", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsManuallyAdjusted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleteReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.CheckConstraint("CK_AttendanceRecords_CheckTimes", "\"CheckOut\" IS NULL OR \"CheckIn\" IS NULL OR \"CheckOut\" >= \"CheckIn\"");
                    table.CheckConstraint("CK_AttendanceRecords_ImportBatch", "\"Source\" <> 'ExcelImport' OR \"ImportBatchId\" IS NOT NULL");
                    table.CheckConstraint("CK_AttendanceRecords_Minutes", "\"WorkingMinutes\" >= 0 AND \"LateMinutes\" >= 0 AND \"EarlyLeaveMinutes\" >= 0 AND \"OvertimeMinutes\" >= 0");
                    table.CheckConstraint("CK_AttendanceRecords_Source", "\"Source\" IN ('ExcelImport', 'Manual', 'DeviceIntegration')");
                    table.CheckConstraint("CK_AttendanceRecords_Status", "\"Status\" IN ('Present', 'Absent', 'Late', 'Leave', 'Holiday', 'Weekend')");
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_AttendanceImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "AttendanceImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalendarExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkingCalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEnglish = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameArabic = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OverrideMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    BreakMinutes = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleteReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarExceptions", x => x.Id);
                    table.CheckConstraint("CK_CalendarExceptions_BreakMinutes", "\"BreakMinutes\" IS NULL OR \"BreakMinutes\" BETWEEN 0 AND 1440");
                    table.CheckConstraint("CK_CalendarExceptions_CustomHours", "\"OverrideMode\" <> 'CustomWorkingHours' OR (\"StartTime\" IS NOT NULL AND \"EndTime\" IS NOT NULL)");
                    table.CheckConstraint("CK_CalendarExceptions_OverrideMode", "\"OverrideMode\" IN ('NonWorkingDay', 'WorkingDay', 'CustomWorkingHours')");
                    table.CheckConstraint("CK_CalendarExceptions_Type", "\"Type\" IN ('OfficialHoliday', 'CompanyHoliday', 'SpecialDay')");
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarExceptions_WorkingCalendars_WorkingCalendarId",
                        column: x => x.WorkingCalendarId,
                        principalTable: "WorkingCalendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkingDaySettings",
                columns: table => new
                {
                    WorkingCalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    BreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    LateGraceMinutes = table.Column<int>(type: "integer", nullable: false),
                    EarlyLeaveGraceMinutes = table.Column<int>(type: "integer", nullable: false),
                    MinimumOvertimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingDaySettings", x => new { x.WorkingCalendarId, x.DayOfWeek });
                    table.CheckConstraint("CK_WorkingDaySettings_BreakMinutes", "\"BreakMinutes\" BETWEEN 0 AND 1440");
                    table.CheckConstraint("CK_WorkingDaySettings_DayOfWeek", "\"DayOfWeek\" BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_WorkingDaySettings_EarlyLeaveGraceMinutes", "\"EarlyLeaveGraceMinutes\" BETWEEN 0 AND 240");
                    table.CheckConstraint("CK_WorkingDaySettings_Hours", "(\"IsWorkingDay\" AND \"StartTime\" IS NOT NULL AND \"EndTime\" IS NOT NULL) OR (NOT \"IsWorkingDay\" AND \"StartTime\" IS NULL AND \"EndTime\" IS NULL)");
                    table.CheckConstraint("CK_WorkingDaySettings_LateGraceMinutes", "\"LateGraceMinutes\" BETWEEN 0 AND 240");
                    table.CheckConstraint("CK_WorkingDaySettings_MinimumOvertimeMinutes", "\"MinimumOvertimeMinutes\" BETWEEN 0 AND 1440");
                    table.CheckConstraint("CK_WorkingDaySettings_NonWorkingValues", "\"IsWorkingDay\" OR (\"BreakMinutes\" = 0 AND \"LateGraceMinutes\" = 0 AND \"EarlyLeaveGraceMinutes\" = 0 AND \"MinimumOvertimeMinutes\" = 0)");
                    table.ForeignKey(
                        name: "FK_WorkingDaySettings_WorkingCalendars_WorkingCalendarId",
                        column: x => x.WorkingCalendarId,
                        principalTable: "WorkingCalendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendancePunches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PunchType = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceRowNumber = table.Column<int>(type: "integer", nullable: true),
                    RawValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RawDataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendancePunches", x => x.Id);
                    table.CheckConstraint("CK_AttendancePunches_PunchType", "\"PunchType\" IN ('CheckIn', 'CheckOut', 'Unknown')");
                    table.CheckConstraint("CK_AttendancePunches_Source", "\"Source\" IN ('ExcelImport', 'Manual', 'DeviceIntegration')");
                    table.CheckConstraint("CK_AttendancePunches_SourceRow", "\"SourceRowNumber\" IS NULL OR \"SourceRowNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_AttendancePunches_AttendanceRecords_AttendanceRecordId",
                        column: x => x.AttendanceRecordId,
                        principalTable: "AttendanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts",
                column: "EmployeeId",
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportBatches_FileHash",
                table: "AttendanceImportBatches",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportBatches_Status_UploadedAt",
                table: "AttendanceImportBatches",
                columns: new[] { "Status", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportBatches_StorageKey",
                table: "AttendanceImportBatches",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportBatches_UploadedByUserId_UploadedAt",
                table: "AttendanceImportBatches",
                columns: new[] { "UploadedByUserId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceImportBatches_ConfirmedFileHash",
                table: "AttendanceImportBatches",
                column: "FileHash",
                unique: true,
                filter: "\"Status\" = 'Confirmed'");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportRows_BatchId_AttendanceDate",
                table: "AttendanceImportRows",
                columns: new[] { "BatchId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportRows_BatchId_CanImport",
                table: "AttendanceImportRows",
                columns: new[] { "BatchId", "CanImport" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportRows_EmployeeId",
                table: "AttendanceImportRows",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceImportRows_SourceEmployeeNumber",
                table: "AttendanceImportRows",
                column: "SourceEmployeeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunches_AttendanceRecordId_Timestamp_PunchType",
                table: "AttendancePunches",
                columns: new[] { "AttendanceRecordId", "Timestamp", "PunchType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunches_Timestamp",
                table: "AttendancePunches",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_AttendanceDate_Status",
                table: "AttendanceRecords",
                columns: new[] { "AttendanceDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_CreatedByUserId_CreatedAt",
                table: "AttendanceRecords",
                columns: new[] { "CreatedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_DeletedByUserId",
                table: "AttendanceRecords",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ImportBatchId_AttendanceDate",
                table: "AttendanceRecords",
                columns: new[] { "ImportBatchId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_MissingCheckOut",
                table: "AttendanceRecords",
                column: "AttendanceDate",
                filter: "\"CheckIn\" IS NOT NULL AND \"CheckOut\" IS NULL AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_UpdatedByUserId",
                table: "AttendanceRecords",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceRecords_Employee_Date",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeId", "AttendanceDate" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_CreatedByUserId",
                table: "CalendarExceptions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_Date_IsActive",
                table: "CalendarExceptions",
                columns: new[] { "Date", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_DeletedByUserId",
                table: "CalendarExceptions",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_Type_Date",
                table: "CalendarExceptions",
                columns: new[] { "Type", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_UpdatedByUserId",
                table: "CalendarExceptions",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_CalendarExceptions_Calendar_Date",
                table: "CalendarExceptions",
                columns: new[] { "WorkingCalendarId", "Date" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveEntitlements_CreatedByUserId",
                table: "EmployeeLeaveEntitlements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveEntitlements_EmployeeId_LeaveTypeId_Year",
                table: "EmployeeLeaveEntitlements",
                columns: new[] { "EmployeeId", "LeaveTypeId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveEntitlements_EmployeeId_Year",
                table: "EmployeeLeaveEntitlements",
                columns: new[] { "EmployeeId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveEntitlements_LeaveTypeId_Year",
                table: "EmployeeLeaveEntitlements",
                columns: new[] { "LeaveTypeId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveEntitlements_UpdatedByUserId",
                table: "EmployeeLeaveEntitlements",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_AttachmentDocumentId",
                table: "LeaveRequests",
                column: "AttachmentDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_CreatedByUserId",
                table: "LeaveRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_DecidedByUserId",
                table: "LeaveRequests",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId_StartDate_EndDate",
                table: "LeaveRequests",
                columns: new[] { "EmployeeId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId_Status",
                table: "LeaveRequests",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_RequestDate",
                table: "LeaveRequests",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Status_StartDate",
                table: "LeaveRequests",
                columns: new[] { "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingCalendars_Name",
                table: "WorkingCalendars",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingCalendars_TimeZoneId",
                table: "WorkingCalendars",
                column: "TimeZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceImportRows");

            migrationBuilder.DropTable(
                name: "AttendancePunches");

            migrationBuilder.DropTable(
                name: "CalendarExceptions");

            migrationBuilder.DropTable(
                name: "EmployeeLeaveEntitlements");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "WorkingDaySettings");

            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "WorkingCalendars");

            migrationBuilder.DropTable(
                name: "AttendanceImportBatches");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts");
        }
    }
}
