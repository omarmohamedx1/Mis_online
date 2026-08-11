using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrAuditService
{
    Task WriteAsync(AuditWriteRequest request, CancellationToken cancellationToken);

    Task<PagedAuditLogsDto> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? action,
        string? entityType,
        Guid? employeeId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken);
}
