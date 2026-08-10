using Microsoft.EntityFrameworkCore;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Repositories;

public sealed class HrAbsenceRepository : IHrAbsenceRepository
{
    private readonly ApplicationDbContext _dbContext;
    public HrAbsenceRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<PagedAbsencesDto> GetPagedAsync(int page, int pageSize, string? search, Guid? departmentId, DateOnly? date, string? status, CancellationToken cancellationToken)
    {
        var query = _dbContext.EmployeeAbsences.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(x => EF.Functions.ILike(x.Employee.EmployeeNumber, $"%{term}%") || EF.Functions.ILike(x.Employee.FullName, $"%{term}%")); }
        if (departmentId.HasValue) query = query.Where(x => x.Employee.DepartmentId == departmentId.Value);
        if (date.HasValue) query = query.Where(x => x.AbsenceDate == date.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.AbsenceDate).ThenBy(x => x.Employee.EmployeeNumber).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AbsenceListItemDto(x.Id, x.EmployeeId, x.Employee.EmployeeNumber, x.Employee.FullName, x.Employee.DepartmentId, x.Employee.Department.Name, x.AbsenceDate, x.Type, x.Status)).ToListAsync(cancellationToken);
        return new PagedAbsencesDto(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public Task<EmployeeAbsence?> GetTrackedAsync(Guid id, CancellationToken cancellationToken) => _dbContext.EmployeeAbsences.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<AbsenceDetailsDto?> GetDetailsAsync(Guid id, CancellationToken cancellationToken) => _dbContext.EmployeeAbsences.AsNoTracking().Where(x => x.Id == id)
        .Select(x => new AbsenceDetailsDto(x.Id, x.EmployeeId, x.Employee.EmployeeNumber, x.Employee.FullName, x.Employee.DepartmentId, x.Employee.Department.Name, x.AbsenceDate, x.Type, x.Reason, x.Status, x.Notes, x.AttendanceSource, x.CreatedAt, x.UpdatedAt)).FirstOrDefaultAsync(cancellationToken);
    public Task<bool> EmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken) => _dbContext.Employees.AnyAsync(x => x.Id == employeeId, cancellationToken);
    public void Add(EmployeeAbsence absence) => _dbContext.EmployeeAbsences.Add(absence);
    public void Remove(EmployeeAbsence absence) => _dbContext.EmployeeAbsences.Remove(absence);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
