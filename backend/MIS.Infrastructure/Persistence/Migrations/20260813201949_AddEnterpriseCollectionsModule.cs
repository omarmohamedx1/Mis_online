using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseCollectionsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionClientOrganizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrganizationType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LogoStorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionClientOrganizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameArabic = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NameEnglish = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SupervisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTeams_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FullNameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FullNameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AlternatePhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AddressArabic = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    AddressEnglish = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    Governorate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Employer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionCustomers_CollectionClientOrganizations_Organizat~",
                        column: x => x.OrganizationId,
                        principalTable: "CollectionClientOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionPortfolios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionPortfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionPortfolios_CollectionClientOrganizations_Organiza~",
                        column: x => x.OrganizationId,
                        principalTable: "CollectionClientOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionTeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTeamMembers_CollectionTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "CollectionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionTeamMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionBucketDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEnglish = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MinimumDays = table.Column<int>(type: "integer", nullable: true),
                    MaximumDays = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionBucketDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionBucketDefinitions_CollectionClientOrganizations_O~",
                        column: x => x.OrganizationId,
                        principalTable: "CollectionClientOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionBucketDefinitions_CollectionPortfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "CollectionPortfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    ValidRows = table.Column<int>(type: "integer", nullable: false),
                    InvalidRows = table.Column<int>(type: "integer", nullable: false),
                    InsertedRows = table.Column<int>(type: "integer", nullable: false),
                    UpdatedRows = table.Column<int>(type: "integer", nullable: false),
                    SkippedRows = table.Column<int>(type: "integer", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionImportBatches_CollectionClientOrganizations_Organ~",
                        column: x => x.OrganizationId,
                        principalTable: "CollectionClientOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionImportBatches_CollectionPortfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "CollectionPortfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionImportBatches_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionUserAccess",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionUserAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionUserAccess_CollectionClientOrganizations_Organiza~",
                        column: x => x.OrganizationId,
                        principalTable: "CollectionClientOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionUserAccess_CollectionPortfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "CollectionPortfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionUserAccess_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    AccountReference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ContractReference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OverdueBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Penalties = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DaysPastDue = table.Column<int>(type: "integer", nullable: false),
                    CurrentBucketId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedCollectorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PriorityScore = table.Column<int>(type: "integer", nullable: false),
                    PriorityExplanation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NextFollowUpAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastContactAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastPaymentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionCases", x => x.Id);
                    table.CheckConstraint("CK_CollectionCases_Amounts", "\"OriginalAmount\" >= 0 AND \"OutstandingBalance\" >= 0 AND \"OverdueBalance\" >= 0");
                    table.CheckConstraint("CK_CollectionCases_Dpd", "\"DaysPastDue\" >= 0");
                    table.ForeignKey(
                        name: "FK_CollectionCases_CollectionBucketDefinitions_CurrentBucketId",
                        column: x => x.CurrentBucketId,
                        principalTable: "CollectionBucketDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionCases_CollectionCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CollectionCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionCases_CollectionPortfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "CollectionPortfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionCases_CollectionTeams_AssignedTeamId",
                        column: x => x.AssignedTeamId,
                        principalTable: "CollectionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionCases_Users_AssignedCollectorId",
                        column: x => x.AssignedCollectorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionImportRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    AccountReference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ContractReference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OutstandingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OverdueBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DaysPastDue = table.Column<int>(type: "integer", nullable: true),
                    RawJson = table.Column<string>(type: "jsonb", nullable: false),
                    ErrorsJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionImportRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionImportRows_CollectionImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "CollectionImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Result = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NextFollowUpAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionActivities_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionActivities_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionAssignmentHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousAssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedById = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionAssignmentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionAssignmentHistory_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAssignmentHistory_CollectionTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "CollectionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAssignmentHistory_Users_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAssignmentHistory_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAssignmentHistory_Users_PreviousAssigneeId",
                        column: x => x.PreviousAssigneeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    BeforeJson = table.Column<string>(type: "jsonb", nullable: true),
                    AfterJson = table.Column<string>(type: "jsonb", nullable: true),
                    Source = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionAuditLogs_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionCaseBucketHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousBucketId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewBucketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionCaseBucketHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionCaseBucketHistory_CollectionBucketDefinitions_New~",
                        column: x => x.NewBucketId,
                        principalTable: "CollectionBucketDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionCaseBucketHistory_CollectionBucketDefinitions_Pre~",
                        column: x => x.PreviousBucketId,
                        principalTable: "CollectionBucketDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionCaseBucketHistory_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionCaseBucketHistory_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionComplaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Source = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SlaDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Resolution = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionComplaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionComplaints_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionComplaints_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionComplaints_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionFieldVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    Governorate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CheckInLatitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CheckInLongitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CheckedInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CheckedOutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionFieldVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionFieldVisits_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionFieldVisits_Users_CollectorId",
                        column: x => x.CollectorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionFieldVisits_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Method = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SubmittedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ProofStorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionPayments", x => x.Id);
                    table.CheckConstraint("CK_CollectionPayments_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_CollectionPayments_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionPayments_Users_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionPayments_Users_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionPromisesToPay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromisedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PromiseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CollectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ActualPaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FulfilledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionPromisesToPay", x => x.Id);
                    table.CheckConstraint("CK_CollectionPromises_Amount", "\"PromisedAmount\" > 0 AND \"ActualPaidAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_CollectionPromisesToPay_CollectionCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "CollectionCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionPromisesToPay_Users_CollectorId",
                        column: x => x.CollectorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionActivities_CaseId_CreatedAt",
                table: "CollectionActivities",
                columns: new[] { "CaseId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionActivities_CreatedById_NextFollowUpAt",
                table: "CollectionActivities",
                columns: new[] { "CreatedById", "NextFollowUpAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAssignmentHistory_AssignedById",
                table: "CollectionAssignmentHistory",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAssignmentHistory_AssignedToId_AssignedAt",
                table: "CollectionAssignmentHistory",
                columns: new[] { "AssignedToId", "AssignedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAssignmentHistory_CaseId_AssignedAt",
                table: "CollectionAssignmentHistory",
                columns: new[] { "CaseId", "AssignedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAssignmentHistory_PreviousAssigneeId",
                table: "CollectionAssignmentHistory",
                column: "PreviousAssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAssignmentHistory_TeamId",
                table: "CollectionAssignmentHistory",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAuditLogs_CaseId_OccurredAt",
                table: "CollectionAuditLogs",
                columns: new[] { "CaseId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAuditLogs_EntityType_EntityId_OccurredAt",
                table: "CollectionAuditLogs",
                columns: new[] { "EntityType", "EntityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAuditLogs_UserId_OccurredAt",
                table: "CollectionAuditLogs",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionBucketDefinitions_OrganizationId_PortfolioId_Code",
                table: "CollectionBucketDefinitions",
                columns: new[] { "OrganizationId", "PortfolioId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionBucketDefinitions_OrganizationId_PortfolioId_Sort~",
                table: "CollectionBucketDefinitions",
                columns: new[] { "OrganizationId", "PortfolioId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionBucketDefinitions_PortfolioId",
                table: "CollectionBucketDefinitions",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCaseBucketHistory_CaseId_ChangedAt",
                table: "CollectionCaseBucketHistory",
                columns: new[] { "CaseId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCaseBucketHistory_ChangedById",
                table: "CollectionCaseBucketHistory",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCaseBucketHistory_NewBucketId",
                table: "CollectionCaseBucketHistory",
                column: "NewBucketId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCaseBucketHistory_PreviousBucketId",
                table: "CollectionCaseBucketHistory",
                column: "PreviousBucketId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_AssignedCollectorId",
                table: "CollectionCases",
                column: "AssignedCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_AssignedTeamId",
                table: "CollectionCases",
                column: "AssignedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_CaseNumber",
                table: "CollectionCases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_CurrentBucketId_DaysPastDue",
                table: "CollectionCases",
                columns: new[] { "CurrentBucketId", "DaysPastDue" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_CustomerId",
                table: "CollectionCases",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_NextFollowUpAt",
                table: "CollectionCases",
                column: "NextFollowUpAt");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_PortfolioId_AccountReference",
                table: "CollectionCases",
                columns: new[] { "PortfolioId", "AccountReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_Status_AssignedCollectorId_PriorityScore",
                table: "CollectionCases",
                columns: new[] { "Status", "AssignedCollectorId", "PriorityScore" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionClientOrganizations_Code",
                table: "CollectionClientOrganizations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionClientOrganizations_IsActive_OrganizationType",
                table: "CollectionClientOrganizations",
                columns: new[] { "IsActive", "OrganizationType" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaints_CaseId",
                table: "CollectionComplaints",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaints_CreatedById",
                table: "CollectionComplaints",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaints_OwnerId_Status",
                table: "CollectionComplaints",
                columns: new[] { "OwnerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaints_Reference",
                table: "CollectionComplaints",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionComplaints_Status_SlaDueAt",
                table: "CollectionComplaints",
                columns: new[] { "Status", "SlaDueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCustomers_NationalId",
                table: "CollectionCustomers",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCustomers_OrganizationId_CustomerCode",
                table: "CollectionCustomers",
                columns: new[] { "OrganizationId", "CustomerCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCustomers_PrimaryPhone",
                table: "CollectionCustomers",
                column: "PrimaryPhone");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFieldVisits_CaseId",
                table: "CollectionFieldVisits",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFieldVisits_CollectorId_ScheduledAt_Status",
                table: "CollectionFieldVisits",
                columns: new[] { "CollectorId", "ScheduledAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFieldVisits_CreatedById",
                table: "CollectionFieldVisits",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionImportBatches_FileHash_PortfolioId",
                table: "CollectionImportBatches",
                columns: new[] { "FileHash", "PortfolioId" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionImportBatches_OrganizationId_UploadedAt",
                table: "CollectionImportBatches",
                columns: new[] { "OrganizationId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionImportBatches_PortfolioId",
                table: "CollectionImportBatches",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionImportBatches_Status_UploadedAt",
                table: "CollectionImportBatches",
                columns: new[] { "Status", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionImportBatches_UploadedById",
                table: "CollectionImportBatches",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionImportRows_BatchId_IsValid",
                table: "CollectionImportRows",
                columns: new[] { "BatchId", "IsValid" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionImportRows_BatchId_RowNumber",
                table: "CollectionImportRows",
                columns: new[] { "BatchId", "RowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPayments_CaseId_PaymentDate",
                table: "CollectionPayments",
                columns: new[] { "CaseId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPayments_ReferenceNumber",
                table: "CollectionPayments",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPayments_Status_SubmittedAt",
                table: "CollectionPayments",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPayments_SubmittedById",
                table: "CollectionPayments",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPayments_VerifiedById",
                table: "CollectionPayments",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPortfolios_OrganizationId_Code",
                table: "CollectionPortfolios",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPromisesToPay_CaseId_PromiseDate",
                table: "CollectionPromisesToPay",
                columns: new[] { "CaseId", "PromiseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPromisesToPay_CollectorId",
                table: "CollectionPromisesToPay",
                column: "CollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPromisesToPay_Status_PromiseDate",
                table: "CollectionPromisesToPay",
                columns: new[] { "Status", "PromiseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTeamMembers_TeamId_UserId",
                table: "CollectionTeamMembers",
                columns: new[] { "TeamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTeamMembers_UserId_IsActive",
                table: "CollectionTeamMembers",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTeams_Code",
                table: "CollectionTeams",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTeams_SupervisorId",
                table: "CollectionTeams",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionUserAccess_OrganizationId",
                table: "CollectionUserAccess",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionUserAccess_PortfolioId",
                table: "CollectionUserAccess",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionUserAccess_UserId_OrganizationId_PortfolioId",
                table: "CollectionUserAccess",
                columns: new[] { "UserId", "OrganizationId", "PortfolioId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionActivities");

            migrationBuilder.DropTable(
                name: "CollectionAssignmentHistory");

            migrationBuilder.DropTable(
                name: "CollectionAuditLogs");

            migrationBuilder.DropTable(
                name: "CollectionCaseBucketHistory");

            migrationBuilder.DropTable(
                name: "CollectionComplaints");

            migrationBuilder.DropTable(
                name: "CollectionFieldVisits");

            migrationBuilder.DropTable(
                name: "CollectionImportRows");

            migrationBuilder.DropTable(
                name: "CollectionPayments");

            migrationBuilder.DropTable(
                name: "CollectionPromisesToPay");

            migrationBuilder.DropTable(
                name: "CollectionTeamMembers");

            migrationBuilder.DropTable(
                name: "CollectionUserAccess");

            migrationBuilder.DropTable(
                name: "CollectionImportBatches");

            migrationBuilder.DropTable(
                name: "CollectionCases");

            migrationBuilder.DropTable(
                name: "CollectionBucketDefinitions");

            migrationBuilder.DropTable(
                name: "CollectionCustomers");

            migrationBuilder.DropTable(
                name: "CollectionTeams");

            migrationBuilder.DropTable(
                name: "CollectionPortfolios");

            migrationBuilder.DropTable(
                name: "CollectionClientOrganizations");
        }
    }
}
