using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Seed;

public static class ApplicationDbSeeder
{
    public static async Task SeedDevelopmentDataAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MIS.Seed");
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        var now = DateTimeOffset.UtcNow;
        var departments = new[]
        {
            ("Human Resources", "الموارد البشرية", DepartmentCodes.Hr),
            ("Legal", "الشؤون القانونية", DepartmentCodes.Legal),
            ("Administration", "الإدارة", DepartmentCodes.Admin),
            ("Data Entry", "إدخال البيانات", DepartmentCodes.DataEntry),
            ("Accounting", "الحسابات", DepartmentCodes.Accounting)
        };

        foreach (var (name, nameArabic, code) in departments)
        {
            var department = await dbContext.Departments.SingleOrDefaultAsync(x => x.Code == code);
            if (department is null)
            {
                dbContext.Departments.Add(new Department(name, code, nameArabic, null, true, now));
            }
            else if (string.IsNullOrWhiteSpace(department.NameArabic))
                department.Update(department.Name, department.Code, nameArabic, department.Description, department.IsActive, now);
        }

        await dbContext.SaveChangesAsync();
        var adminDepartment = await dbContext.Departments.SingleAsync(x => x.Code == DepartmentCodes.Admin);
        var hrDepartment = await dbContext.Departments.SingleAsync(x => x.Code == DepartmentCodes.Hr);
        await SeedHrMasterDataAsync(dbContext, hrDepartment.Id, now);
        var adminRole = await dbContext.Roles.SingleOrDefaultAsync(role => role.Name == SystemRoleNames.Admin);

        if (adminRole is null)
        {
            adminRole = new Role(SystemRoleNames.Admin, "System administrator role", true, now);
            dbContext.Roles.Add(adminRole);
        }

        var adminPassword = configuration["Seed:AdminPassword"];
        var username = configuration["Seed:AdminUsername"] ?? "admin";
        var email = configuration["Seed:AdminEmail"] ?? "admin@mis.local";
        var fullName = configuration["Seed:AdminFullName"] ?? "MIS Administrator";

        var adminUser = await dbContext.Users
            .Include(user => user.UserRoles)
            .SingleOrDefaultAsync(user => user.Username == username);

        if (adminUser is null && !string.IsNullOrWhiteSpace(adminPassword))
        {
            adminUser = new User(username, email, "temporary-seed-hash", fullName, adminDepartment.Id, now);
            dbContext.Users.Add(adminUser);
        }

        if (adminUser is not null && !string.IsNullOrWhiteSpace(adminPassword))
        {
            adminUser.SetPasswordHash(new PasswordHasher<User>().HashPassword(adminUser, adminPassword), now);
            adminUser.AssignRole(adminRole, now);
        }

        var hrPassword = configuration["Seed:HrPassword"];
        var hrUsername = configuration["Seed:HrUsername"] ?? "hr.user";
        var hrUser = await dbContext.Users.Include(x => x.UserRoles).SingleOrDefaultAsync(x => x.Username == hrUsername);
        if (hrUser is null && !string.IsNullOrWhiteSpace(hrPassword))
        {
            hrUser = new User(
                hrUsername,
                configuration["Seed:HrEmail"] ?? "hr@mis.local",
                "temporary-seed-hash",
                configuration["Seed:HrFullName"] ?? "HR User",
                hrDepartment.Id,
                now);
            dbContext.Users.Add(hrUser);
        }

        if (hrUser is not null && !string.IsNullOrWhiteSpace(hrPassword))
        {
            hrUser.SetPasswordHash(new PasswordHasher<User>().HashPassword(hrUser, hrPassword), now);
        }

        if (string.IsNullOrWhiteSpace(adminPassword) && string.IsNullOrWhiteSpace(hrPassword))
        {
            logger.LogWarning("No development users were seeded because seed passwords are not configured.");
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedHrMasterDataAsync(ApplicationDbContext dbContext, Guid hrDepartmentId, DateTimeOffset now)
    {
        var mainBranch = await dbContext.Branches.SingleOrDefaultAsync(item => item.Code == "MAIN");
        if (mainBranch is null)
            dbContext.Branches.Add(new Branch("Main Branch", "MAIN", "الفرع الرئيسي", null, null, true, now));
        else if (string.IsNullOrWhiteSpace(mainBranch.NameArabic))
            mainBranch.Update(mainBranch.Name, mainBranch.Code, "الفرع الرئيسي", mainBranch.Description, mainBranch.Address, mainBranch.IsActive, now);

        var employmentTypes = new[]
        {
            ("Full Time", "دوام كامل", "FULL_TIME"),
            ("Part Time", "دوام جزئي", "PART_TIME"),
            ("Temporary", "مؤقت", "TEMPORARY"),
            ("Internship", "تدريب", "INTERNSHIP")
        };
        foreach (var (name, nameArabic, code) in employmentTypes)
        {
            var item = await dbContext.EmploymentTypes.SingleOrDefaultAsync(value => value.Code == code);
            if (item is null)
                dbContext.EmploymentTypes.Add(new EmploymentType(name, code, nameArabic, null, true, now));
            else if (string.IsNullOrWhiteSpace(item.NameArabic))
                item.Update(item.Name, item.Code, nameArabic, item.Description, item.IsActive, now);
        }

        var contractTypes = new[]
        {
            ("Permanent", "دائم", "PERMANENT"),
            ("Fixed Term", "محدد المدة", "FIXED_TERM"),
            ("Project Based", "مرتبط بمشروع", "PROJECT_BASED")
        };
        foreach (var (name, nameArabic, code) in contractTypes)
        {
            var item = await dbContext.ContractTypes.SingleOrDefaultAsync(value => value.Code == code);
            if (item is null)
                dbContext.ContractTypes.Add(new ContractType(name, code, nameArabic, null, true, now));
            else if (string.IsNullOrWhiteSpace(item.NameArabic))
                item.Update(item.Name, item.Code, nameArabic, item.Description, item.IsActive, now);
        }

        var leaveTypes = new[]
        {
            ("Annual Leave", "إجازة سنوية", "ANNUAL", 21m, false),
            ("Sick Leave", "إجازة مرضية", "SICK", 0m, true),
            ("Emergency Leave", "إجازة طارئة", "EMERGENCY", 0m, false),
            ("Unpaid Leave", "إجازة بدون راتب", "UNPAID", 0m, false),
            ("Maternity Leave", "إجازة وضع", "MATERNITY", 0m, true),
            ("Permission", "إذن", "PERMISSION", 0m, false),
            ("Early Leave", "انصراف مبكر", "EARLY_LEAVE", 0m, false)
        };
        foreach (var (name, nameArabic, code, entitlement, requiresAttachment) in leaveTypes)
        {
            var item = await dbContext.LeaveTypes.SingleOrDefaultAsync(value => value.Code == code);
            if (item is null)
                dbContext.LeaveTypes.Add(new LeaveType(name, code, nameArabic, null, entitlement, requiresAttachment, true, now));
            else if (string.IsNullOrWhiteSpace(item.NameArabic))
                item.Update(item.Name, item.Code, nameArabic, item.Description, item.DefaultAnnualEntitlement, item.RequiresAttachment, item.IsActive, now);
        }

        var documentTypes = new[]
        {
            ("National ID", "بطاقة الرقم القومي", "NATIONAL_ID", true),
            ("Contract", "عقد العمل", "CONTRACT", true),
            ("Graduation Certificate", "شهادة التخرج", "GRADUATION_CERTIFICATE", false),
            ("Military Certificate", "شهادة الموقف من التجنيد", "MILITARY_CERTIFICATE", false),
            ("Insurance Document", "مستند التأمينات", "INSURANCE_DOCUMENT", true),
            ("Medical Document", "مستند طبي", "MEDICAL_DOCUMENT", true),
            ("CV", "السيرة الذاتية", "CV", false),
            ("Other", "أخرى", "OTHER", false)
        };
        foreach (var (name, nameArabic, code, requiresExpiry) in documentTypes)
        {
            var item = await dbContext.DocumentTypes.SingleOrDefaultAsync(value => value.Code == code);
            if (item is null)
                dbContext.DocumentTypes.Add(new DocumentType(name, code, nameArabic, null, requiresExpiry, true, now));
            else if (string.IsNullOrWhiteSpace(item.NameArabic))
                item.Update(item.Name, item.Code, nameArabic, item.Description, item.RequiresExpiryDate, item.IsActive, now);
        }

        var delegationTypes = new[]
        {
            ("Cheque Collection", "استلام شيكات", "CHEQUE_COLLECTION"),
            ("Document Collection", "استلام مستندات", "DOCUMENT_COLLECTION"),
            ("Government Procedures", "إجراءات حكومية", "GOVERNMENT_PROCEDURES"),
            ("General Administrative", "تفويض إداري عام", "GENERAL_ADMINISTRATIVE")
        };
        foreach (var (name, nameArabic, code) in delegationTypes)
        {
            var item = await dbContext.DelegationTypes.SingleOrDefaultAsync(value => value.Code == code);
            if (item is null)
                dbContext.DelegationTypes.Add(new DelegationType(name, code, nameArabic, null, true, now));
            else if (string.IsNullOrWhiteSpace(item.NameArabic))
                item.Update(item.Name, item.Code, nameArabic, item.Description, item.IsActive, now);
        }

        var positions = new[]
        {
            ("HR Manager", "مدير الموارد البشرية", "HR_MANAGER"),
            ("HR Officer", "مسؤول موارد بشرية", "HR_OFFICER")
        };
        foreach (var (name, nameArabic, code) in positions)
        {
            var item = await dbContext.Positions.SingleOrDefaultAsync(value => value.Code == code);
            if (item is null)
                dbContext.Positions.Add(new Position(name, code, nameArabic, null, hrDepartmentId, true, now));
            else if (string.IsNullOrWhiteSpace(item.NameArabic))
                item.Update(item.Name, item.Code, nameArabic, item.Description, item.DepartmentId, item.IsActive, now);
        }

        if (!await dbContext.WorkingCalendars.AnyAsync())
        {
            var calendar = new WorkingCalendar("Default company calendar", "Africa/Cairo", now);
            foreach (var day in Enum.GetValues<DayOfWeek>())
            {
                var isWorkingDay = day is not DayOfWeek.Friday and not DayOfWeek.Saturday;
                calendar.SetDay(
                    day,
                    isWorkingDay,
                    isWorkingDay ? new TimeOnly(9, 0) : null,
                    isWorkingDay ? new TimeOnly(17, 0) : null,
                    isWorkingDay ? 60 : 0,
                    isWorkingDay ? 15 : 0,
                    isWorkingDay ? 15 : 0,
                    isWorkingDay ? 30 : 0,
                    now);
            }

            dbContext.WorkingCalendars.Add(calendar);
        }

        await dbContext.SaveChangesAsync();
    }
}
