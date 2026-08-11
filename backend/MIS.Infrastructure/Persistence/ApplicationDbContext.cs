using Microsoft.EntityFrameworkCore;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    public DbSet<ContractType> ContractTypes => Set<ContractType>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<DelegationType> DelegationTypes => Set<DelegationType>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeContract> EmployeeContracts => Set<EmployeeContract>();
    public DbSet<EmployeeCompensation> EmployeeCompensations => Set<EmployeeCompensation>();
    public DbSet<EmployeeEmergencyContact> EmployeeEmergencyContacts => Set<EmployeeEmergencyContact>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmployeeAbsence> EmployeeAbsences => Set<EmployeeAbsence>();
    public DbSet<HrAuditLog> HrAuditLogs => Set<HrAuditLog>();
    public DbSet<WorkingCalendar> WorkingCalendars => Set<WorkingCalendar>();
    public DbSet<WorkingDaySetting> WorkingDaySettings => Set<WorkingDaySetting>();
    public DbSet<CalendarException> CalendarExceptions => Set<CalendarException>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendancePunch> AttendancePunches => Set<AttendancePunch>();
    public DbSet<AttendanceImportBatch> AttendanceImportBatches => Set<AttendanceImportBatch>();
    public DbSet<AttendanceImportRow> AttendanceImportRows => Set<AttendanceImportRow>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<EmployeeLeaveEntitlement> EmployeeLeaveEntitlements => Set<EmployeeLeaveEntitlement>();
    public DbSet<EmployeeDelegation> EmployeeDelegations => Set<EmployeeDelegation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
