using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Repositories;

public sealed class HrEmployeeRepository : IHrEmployeeRepository
{
    private readonly ApplicationDbContext _dbContext;
    public HrEmployeeRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public Task<PagedEmployeesDto> GetPagedAsync(int page, int pageSize, string? search, Guid? departmentId, bool? isActive, CancellationToken cancellationToken) =>
        GetPagedCoreAsync(page, pageSize, search, departmentId, isActive, null, cancellationToken);

    public Task<PagedEmployeesDto> GetPagedByStatusAsync(int page, int pageSize, string? search, Guid? departmentId, string? status, CancellationToken cancellationToken) =>
        GetPagedCoreAsync(page, pageSize, search, departmentId, null, status, cancellationToken);

    private async Task<PagedEmployeesDto> GetPagedCoreAsync(
        int page,
        int pageSize,
        string? search,
        Guid? departmentId,
        bool? isActive,
        string? status,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var query = _dbContext.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(employee =>
                EF.Functions.ILike(employee.EmployeeNumber, $"%{term}%") ||
                EF.Functions.ILike(employee.FullName, $"%{term}%") ||
                (employee.FullNameArabic != null && EF.Functions.ILike(employee.FullNameArabic, $"%{term}%")) ||
                (employee.FullNameEnglish != null && EF.Functions.ILike(employee.FullNameEnglish, $"%{term}%")));
        }
        if (departmentId.HasValue) query = query.Where(employee => employee.DepartmentId == departmentId.Value);
        if (isActive.HasValue) query = query.Where(employee => employee.IsActive == isActive.Value);
        else if (!string.IsNullOrWhiteSpace(status)) query = query.Where(employee => employee.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(employee => employee.EmployeeNumber).ThenBy(employee => employee.FullName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(employee => new EmployeeListItemDto(
                employee.Id,
                employee.EmployeeNumber,
                isArabic ? employee.FullNameArabic ?? employee.FullName : employee.FullNameEnglish ?? employee.FullName,
                employee.DepartmentId,
                isArabic ? employee.Department.NameArabic ?? employee.Department.Name : employee.Department.Name,
                employee.Department.Code,
                employee.IsActive,
                employee.Status))
            .ToListAsync(cancellationToken);
        return new PagedEmployeesDto(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public Task<Employee?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);

    public Task<EmployeeDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return _dbContext.Employees.AsNoTracking().Where(employee => employee.Id == id)
            .Select(employee => new EmployeeDetailsDto(
                employee.Id,
                employee.EmployeeNumber,
                isArabic ? employee.FullNameArabic ?? employee.FullName : employee.FullNameEnglish ?? employee.FullName,
                employee.DepartmentId,
                isArabic ? employee.Department.NameArabic ?? employee.Department.Name : employee.Department.Name,
                employee.Department.Code,
                employee.IsActive,
                employee.CreatedAt,
                employee.UpdatedAt,
                employee.Status))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DepartmentOptionDto>> GetDepartmentsAsync(CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return await _dbContext.Departments.AsNoTracking()
            .OrderBy(department => isArabic ? department.NameArabic ?? department.Name : department.Name)
            .Select(department => new DepartmentOptionDto(
                department.Id,
                isArabic ? department.NameArabic ?? department.Name : department.Name,
                department.Code))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken) =>
        _dbContext.Departments.AnyAsync(department => department.Id == departmentId, cancellationToken);

    public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalized = employeeNumber.Trim().ToLower();
        return _dbContext.Employees.AnyAsync(employee => employee.EmployeeNumber.ToLower() == normalized && (!excludingId.HasValue || employee.Id != excludingId.Value), cancellationToken);
    }

    public void Add(Employee employee) => _dbContext.Employees.Add(employee);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
