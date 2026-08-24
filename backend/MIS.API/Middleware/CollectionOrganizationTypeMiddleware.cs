using Microsoft.EntityFrameworkCore;
using MIS.Domain.Constants;
using MIS.Infrastructure.Persistence;

namespace MIS.API.Middleware;

public sealed class CollectionOrganizationTypeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (segments.Length >= 3 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(segments[2], out var organizationId) &&
                (segments[1].Equals("banks", StringComparison.OrdinalIgnoreCase) || segments[1].Equals("installment-companies", StringComparison.OrdinalIgnoreCase)))
            {
                var expectedType = segments[1].Equals("banks", StringComparison.OrdinalIgnoreCase)
                    ? CollectionsValues.OrganizationTypes.Bank
                    : CollectionsValues.OrganizationTypes.ConsumerFinance;
                var matches = await db.CollectionClientOrganizations.AsNoTracking().AnyAsync(x => x.Id == organizationId && x.IsActive && x.OrganizationType == expectedType, context.RequestAborted);
                if (!matches) { context.Response.StatusCode = StatusCodes.Status404NotFound; return; }
            }
        }
        await next(context);
    }
}
