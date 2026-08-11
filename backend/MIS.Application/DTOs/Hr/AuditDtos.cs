namespace MIS.Application.DTOs.Hr;

public sealed record AuditChangeDto(string Field, string? OldValue, string? NewValue);

public sealed record AuditLogItemDto(
    Guid Id,
    Guid UserId,
    string Username,
    string Action,
    string EntityType,
    string EntityId,
    Guid? EmployeeId,
    string? EmployeeName,
    string? Description,
    IReadOnlyCollection<AuditChangeDto> Changes,
    DateTimeOffset Timestamp);

public sealed record PagedAuditLogsDto(
    IReadOnlyCollection<AuditLogItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record AuditWriteRequest(
    string Action,
    string EntityType,
    string EntityId,
    Guid? EmployeeId,
    object? OldValue,
    object? NewValue,
    string? Description);
