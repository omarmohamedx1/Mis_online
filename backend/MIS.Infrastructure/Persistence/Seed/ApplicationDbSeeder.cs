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
            ("Human Resources", DepartmentCodes.Hr),
            ("Legal", DepartmentCodes.Legal),
            ("Administration", DepartmentCodes.Admin),
            ("Data Entry", DepartmentCodes.DataEntry),
            ("Accounting", DepartmentCodes.Accounting)
        };

        foreach (var (name, code) in departments)
        {
            if (!await dbContext.Departments.AnyAsync(x => x.Code == code))
            {
                dbContext.Departments.Add(new Department(name, code, now));
            }
        }

        await dbContext.SaveChangesAsync();
        var adminDepartment = await dbContext.Departments.SingleAsync(x => x.Code == DepartmentCodes.Admin);
        var hrDepartment = await dbContext.Departments.SingleAsync(x => x.Code == DepartmentCodes.Hr);
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
}
