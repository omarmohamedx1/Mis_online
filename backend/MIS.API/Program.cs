using System.Text;
using MIS.API.Configuration;
using MIS.API.Middleware;
using MIS.Infrastructure.Authentication;
using MIS.Infrastructure.Persistence.Seed;

using var bootstrapLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var bootstrapLogger = bootstrapLoggerFactory.CreateLogger("MIS.Startup");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // The Windows EventLog provider can be registered by the default host even when the
    // process identity cannot write to the Event Log. A logging failure must never mask
    // the original API exception, so keep the configured cross-platform providers only.
    builder.Logging.ClearProviders();
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    LogConfigurationStatus(bootstrapLogger, builder);
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("MIS.Startup");
    var swaggerEnabled = IsEnabled(app.Configuration["Swagger:Enabled"]);

    // Every response uses the language requested by the web client (Accept-Language: ar|en).
    // Keep this before error handling/authentication so validation, 401, 403, and business
    // errors all use the same request culture.
    app.UseRequestLocalization();
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("Vary", "Accept-Language");
        await next();
    });
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (swaggerEnabled)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors(ApiServiceCollectionExtensions.FrontendCorsPolicy);
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<CollectionOrganizationTypeMiddleware>();

    app.MapControllers();
    app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();

    // Make the development API address friendly when opened directly in a browser.
    // The frontend remains the owner of the login UI.
    if (app.Environment.IsDevelopment())
    {
        app.MapGet("/", () => Results.Redirect("http://localhost:5173/login"))
            .AllowAnonymous();
    }

    if (app.Environment.IsDevelopment())
    {
        startupLogger.LogInformation("Database migrations: enabled as part of development seeding.");
        startupLogger.LogInformation("Database seeding: starting development-only idempotent seed.");
        await ApplicationDbSeeder.SeedDevelopmentDataAsync(app.Services, app.Configuration);
        startupLogger.LogInformation("Database migrations and development seeding completed successfully.");
    }
    else
    {
        startupLogger.LogInformation("Database migrations: skipped. Automatic migrations are disabled outside Development.");
        startupLogger.LogInformation("Database seeding: skipped. Development seeding is disabled outside Development.");
    }

    startupLogger.LogInformation("Swagger UI enabled: {SwaggerEnabled}.", swaggerEnabled);
    app.Lifetime.ApplicationStarted.Register(() =>
        startupLogger.LogInformation("MIS API started successfully in {Environment}.", app.Environment.EnvironmentName));

    await app.RunAsync();
}
catch (Exception exception)
{
    bootstrapLogger.LogCritical(exception, "MIS API startup failed.");
    throw;
}

static void LogConfigurationStatus(ILogger logger, WebApplicationBuilder builder)
{
    var configuration = builder.Configuration;
    var jwtSecret = configuration[$"{JwtOptions.SectionName}:SecretKey"];
    var jwtReady = !string.IsNullOrWhiteSpace(jwtSecret) &&
        Encoding.UTF8.GetByteCount(jwtSecret) >= JwtOptions.MinimumSecretBytes;
    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    logger.LogInformation("Starting MIS API. Environment: {Environment}.", builder.Environment.EnvironmentName);
    logger.LogInformation(
        "Database connection string configured: {Configured}.",
        !string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")));
    logger.LogInformation("JWT secret configured and meets the minimum length: {Configured}.", jwtReady);
    logger.LogInformation("CORS allowed origin count: {OriginCount}.", allowedOrigins.Length);
    if (!builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        logger.LogWarning("No production CORS origins are configured; browser frontends on another origin will be blocked.");
    logger.LogInformation(
        "File storage root configured: {Configured}.",
        !string.IsNullOrWhiteSpace(configuration["HrFiles:RootPath"]));

    var swaggerValue = configuration["Swagger:Enabled"];
    if (!string.IsNullOrWhiteSpace(swaggerValue) && !bool.TryParse(swaggerValue, out _))
        logger.LogWarning("Swagger:Enabled is invalid; Swagger will remain disabled. Use true or false.");
}

static bool IsEnabled(string? value) => bool.TryParse(value, out var enabled) && enabled;

public partial class Program;
