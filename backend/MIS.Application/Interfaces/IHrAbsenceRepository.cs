using MIS.Application.DTOs.Hr;
using MIS.Domain.Entities;

namespace MIS.Application.Interfaces;

public interface IHrAbsenceRepository
{
    Task<PagedAbsencesDto> GetPagedAsync(int page, int pageSize, string? search, Guid? departmentId, DateOnly? date, string? status, CancellationToken cancellationToken);
    Task<EmployeeAbsence?> GetTrackedAsync(Guid id, CancellationToken cancellationToken);
    Task<AbsenceDetailsDto?> GetDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken);
    void Add(EmployeeAbsence absence);
    void Remove(EmployeeAbsence absence);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
