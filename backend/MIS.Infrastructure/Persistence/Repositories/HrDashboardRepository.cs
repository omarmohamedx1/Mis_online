using Microsoft.EntityFrameworkCore;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.Infrastructure.Persistence.Repositories;

public sealed class HrDashboardRepository : IHrDashboardRepository
{
    private readonly ApplicationDbContext _dbContext;

    public HrDashboardRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HrDashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var totalEmployees = await _dbContext.Employees
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeEmployees = await _dbContext.Employees
            .AsNoTracking()
            .CountAsync(employee => employee.IsActive, cancellationToken);

        var totalDocuments = await _dbContext.EmployeeDocuments
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var departmentCounts = await _dbContext.Departments
            .AsNoTracking()
            .Select(department => new
            {
                department.Id,
                department.Name,
                department.Code,
                EmployeeCount = _dbContext.Employees.Count(employee => employee.DepartmentId == department.Id)
            })
            .OrderByDescending(item => item.EmployeeCount)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);

        var employeesByDepartment = departmentCounts
            .Select(item => new DepartmentEmployeeCountDto(
                item.Id,
                item.Name,
                item.Code,
                item.EmployeeCount))
            .ToArray();

        // Attendance and document-attention rules do not exist in the current schema.
        return new HrDashboardDto(
            totalEmployees,
            activeEmployees,
            null,
            false,
            null,
            false,
            totalDocuments,
            employeesByDepartment);
    }
}
