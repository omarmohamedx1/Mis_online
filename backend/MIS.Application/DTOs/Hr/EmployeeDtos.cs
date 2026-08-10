using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Hr;

public sealed record EmployeeListItemDto(Guid Id, string EmployeeNumber, string FullName, Guid DepartmentId, string DepartmentName, string DepartmentCode, bool IsActive);
public sealed record EmployeeDetailsDto(Guid Id, string EmployeeNumber, string FullName, Guid DepartmentId, string DepartmentName, string DepartmentCode, bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record DepartmentOptionDto(Guid Id, string Name, string Code);
public sealed record PagedEmployeesDto(IReadOnlyCollection<EmployeeListItemDto> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public sealed class SaveEmployeeRequest
{
    [Required, StringLength(50, MinimumLength = 1)]
    public string EmployeeNumber { get; init; } = string.Empty;
    [Required, StringLength(160, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;
    [Required]
    public Guid DepartmentId { get; init; }
    public bool IsActive { get; init; } = true;
}
