using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIS.Application.Interfaces;
using MIS.Infrastructure.Authentication;
using MIS.Infrastructure.Persistence;
using MIS.Infrastructure.Persistence.Repositories;

namespace MIS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        ValidateJwtOptions(configuration);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }

    private static void ValidateJwtOptions(IConfiguration configuration)
    {
        var secretKey = configuration[$"{JwtOptions.SectionName}:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < JwtOptions.MinimumSecretBytes)
        {
            throw new InvalidOperationException($"Jwt:SecretKey must be configured with at least {JwtOptions.MinimumSecretBytes} bytes. Use environment variables or user secrets.");
        }
    }
}
