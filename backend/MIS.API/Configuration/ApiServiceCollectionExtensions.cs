using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MIS.Application.Common;
using MIS.Application.Interfaces;
using MIS.Application.Services;
using MIS.Infrastructure;
using MIS.Infrastructure.Authentication;
using MIS.API.Authorization;
using MIS.Domain.Constants;

namespace MIS.API.Configuration;

public static class ApiServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "FrontendCorsPolicy";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(modelState => modelState.Value?.Errors.Count > 0)
                        .SelectMany(modelState => modelState.Value!.Errors)
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid request value." : error.ErrorMessage)
                        .ToArray();

                    return new BadRequestObjectResult(ApiErrorResponse.Failure("Validation failed.", errors));
                };
            });

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddInfrastructureServices(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.HrDepartment,
                policy => policy.RequireAuthenticatedUser().RequireClaim("department", DepartmentCodes.Hr));
        });

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is missing.");

        var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(ApiErrorResponse.Failure("Authentication is required."));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(ApiErrorResponse.Failure("You are not authorized to access this resource."));
                    }
                };
            });

        return services;
    }
}
