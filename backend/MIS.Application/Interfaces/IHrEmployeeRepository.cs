using MIS.Application.DTOs.Hr;
using MIS.Domain.Entities;

namespace MIS.Application.Interfaces;

public interface IHrEmployeeRepository
{
    Task<PagedEmployeesDto> GetPagedAsync(int page, int pageSize, string? search, Guid? departmentId, bool? isActive, CancellationToken cancellationToken);
    Task<Employee?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<EmployeeDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DepartmentOptionDto>> GetDepartmentsAsync(CancellationToken cancellationToken);
    Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken);
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber, Guid? excludingId, CancellationToken cancellationToken);
    void Add(Employee employee);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
