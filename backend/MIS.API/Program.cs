using MIS.API.Configuration;
using MIS.API.Middleware;
using MIS.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(ApiServiceCollectionExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    await ApplicationDbSeeder.SeedDevelopmentDataAsync(app.Services, app.Configuration);
}

app.Run();

public partial class Program;
