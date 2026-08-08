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

        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Development admin user was not seeded because Seed:AdminPassword is not configured.");
            return;
        }

        await dbContext.Database.MigrateAsync();

        var now = DateTimeOffset.UtcNow;
        var adminRole = await dbContext.Roles.SingleOrDefaultAsync(role => role.Name == SystemRoleNames.Admin);

        if (adminRole is null)
        {
            adminRole = new Role(SystemRoleNames.Admin, "System administrator role", true, now);
            dbContext.Roles.Add(adminRole);
        }

        var username = configuration["Seed:AdminUsername"] ?? "admin";
        var email = configuration["Seed:AdminEmail"] ?? "admin@mis.local";
        var fullName = configuration["Seed:AdminFullName"] ?? "MIS Administrator";

        var adminUser = await dbContext.Users
            .Include(user => user.UserRoles)
            .SingleOrDefaultAsync(user => user.Username == username);

        if (adminUser is null)
        {
            adminUser = new User(username, email, "temporary-seed-hash", fullName, now);
            dbContext.Users.Add(adminUser);
        }

        var passwordHash = new PasswordHasher<User>().HashPassword(adminUser, adminPassword);
        adminUser.SetPasswordHash(passwordHash, now);
        adminUser.AssignRole(adminRole, now);

        await dbContext.SaveChangesAsync();
    }
}
