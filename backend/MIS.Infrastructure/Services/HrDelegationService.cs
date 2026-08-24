using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Constants;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;

namespace MIS.Infrastructure.Services;

public sealed class HrDelegationService : IHrDelegationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly IHrAuditService _audit;

    public HrDelegationService(ApplicationDbContext dbContext, ICurrentUserContext currentUser, IHrAuditService audit)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PagedDelegationsDto> GetPagedAsync(DelegationFilterDto filter, CancellationToken cancellationToken)
    {
        ValidateFilter(filter);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = BaseQuery().AsNoTracking();
        if (filter.EmployeeId.HasValue) query = query.Where(item => item.EmployeeId == filter.EmployeeId.Value);
        if (filter.DepartmentId.HasValue) query = query.Where(item => item.Employee.DepartmentId == filter.DepartmentId.Value);
        if (filter.DelegationTypeId.HasValue) query = query.Where(item => item.DelegationTypeId == filter.DelegationTypeId.Value);
        if (filter.DelegatingEntityId.HasValue) query = query.Where(item => item.DelegatingEntityId == filter.DelegatingEntityId.Value);
        if (filter.DateFrom.HasValue) query = query.Where(item => item.EndDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(item => item.StartDate <= filter.DateTo.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(item =>
                item.DelegationNumber.ToLower().Contains(term) ||
                item.Employee.EmployeeNumber.ToLower().Contains(term) ||
                item.Employee.FullName.ToLower().Contains(term) ||
                (item.Employee.FullNameArabic != null && item.Employee.FullNameArabic.ToLower().Contains(term)) ||
                (item.Employee.FullNameEnglish != null && item.Employee.FullNameEnglish.ToLower().Contains(term)) ||
                (item.Employee.NationalId != null && item.Employee.NationalId.Contains(term)) ||
                (item.EmployeeNationalIdSnapshot != null && item.EmployeeNationalIdSnapshot.Contains(term)) ||
                item.Subject.ToLower().Contains(term) ||
                (item.AuthorizedEntity != null && item.AuthorizedEntity.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = NormalizeStatus(filter.Status);
            query = status switch
            {
                DelegationStatuses.Expired => query.Where(item => item.Status == DelegationStatuses.Expired ||
                    (item.Status == DelegationStatuses.Active && item.EndDate < today)),
                DelegationStatuses.Active => query.Where(item => item.Status == DelegationStatuses.Active && item.EndDate >= today),
                _ => query.Where(item => item.Status == status)
            };
        }

        var total = await query.CountAsync(cancellationToken);
        query = ApplySort(query, filter.SortBy, filter.SortDirection);
        var entities = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToArrayAsync(cancellationToken);
        return new PagedDelegationsDto(
            entities.Select(item => MapList(item, today)).ToArray(),
            total,
            filter.Page,
            filter.PageSize,
            Pages(total, filter.PageSize));
    }

    public async Task<IReadOnlyCollection<DelegationEntityOptionDto>> GetEntitiesAsync(CancellationToken cancellationToken) =>
        await _dbContext.CollectionClientOrganizations.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.NameArabic)
            .Select(item => new DelegationEntityOptionDto(item.Id, item.NameArabic, item.NameEnglish))
            .ToArrayAsync(cancellationToken);

    public async Task<DelegationDetailsDto> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await BaseQuery().AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new HrNotFoundException("Delegation was not found.");
        return MapDetails(entity, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<DelegationDetailsDto> CreateAsync(CreateDelegationRequest request, CancellationToken cancellationToken)
    {
        ValidateDates(request.StartDate, request.EndDate);
        var employee = await GetActiveEmployeeAsync(request.EmployeeId, cancellationToken);
        if (string.IsNullOrWhiteSpace(employee.NationalId))
            throw new HrValidationException("The selected employee has no National ID. Add it to the employee profile before generating a delegation.");
        ValidateEmployeeDates(employee, request.StartDate, request.EndDate);
        var type = request.DelegationTypeId.HasValue
            ? await GetActiveTypeAsync(request.DelegationTypeId.Value, cancellationToken)
            : await GetDefaultTypeAsync(cancellationToken);
        var entityName = await ResolveEntityNameAsync(request.DelegatingEntityId, request.AuthorizedEntity, cancellationToken);
        var number = $"DEL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..23].ToUpperInvariant();
        if (await _dbContext.EmployeeDelegations.AnyAsync(item => item.DelegationNumber == number, cancellationToken))
            throw new HrConflictException("Delegation number already exists.");

        var entity = new EmployeeDelegation(
            number,
            employee.Id,
            type.Id,
            string.IsNullOrWhiteSpace(request.Subject) ? "تفويض تحصيل" : request.Subject,
            request.DelegatingEntityId,
            entityName,
            request.CompanyRepresentative,
            request.PowerOfAttorneyNumber,
            request.PowerOfAttorneyYear,
            employee.FullNameArabic ?? employee.FullName,
            employee.EmployeeNumber,
            employee.NationalId,
            request.StartDate,
            request.EndDate,
            request.Purpose,
            request.Notes,
            NormalizeStatus(request.Status),
            _currentUser.UserId,
            DateTimeOffset.UtcNow);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.EmployeeDelegations.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "DelegationCreated",
            nameof(EmployeeDelegation),
            entity.Id.ToString(),
            employee.Id,
            null,
            new { entity.DelegationNumber, Type = type.Name, entity.Subject, entity.StartDate, entity.EndDate, entity.Status },
            $"Created delegation {entity.DelegationNumber} for employee {employee.EmployeeNumber}."), cancellationToken);
        var created = await GetDetailsAsync(entity.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return created;
    }

    public async Task<DelegationDetailsDto> UpdateAsync(Guid id, UpdateDelegationRequest request, CancellationToken cancellationToken)
    {
        ValidateDates(request.StartDate, request.EndDate);
        var entity = await GetTrackedAsync(id, cancellationToken);
        if (request.EmployeeId.HasValue && request.EmployeeId.Value != entity.EmployeeId)
            throw new HrValidationException("The employee on an issued delegation cannot be changed. Create a new delegation instead.");
        var employee = await GetActiveEmployeeAsync(entity.EmployeeId, cancellationToken);
        ValidateEmployeeDates(employee, request.StartDate, request.EndDate);
        var typeId = request.DelegationTypeId ?? entity.DelegationTypeId;
        if (typeId != entity.DelegationTypeId) await GetActiveTypeAsync(typeId, cancellationToken);
        var entityName = await ResolveEntityNameAsync(request.DelegatingEntityId, request.AuthorizedEntity, cancellationToken);
        var oldValue = Snapshot(entity);
        try
        {
            entity.Update(
                typeId,
                string.IsNullOrWhiteSpace(request.Subject) ? entity.Subject : request.Subject,
                request.DelegatingEntityId,
                entityName,
                request.CompanyRepresentative,
                request.PowerOfAttorneyNumber,
                request.PowerOfAttorneyYear,
                request.StartDate,
                request.EndDate,
                request.Purpose,
                request.Notes,
                NormalizeStatus(request.Status),
                _currentUser.UserId,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new HrConflictException(exception.Message);
        }
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "DelegationUpdated",
            nameof(EmployeeDelegation),
            entity.Id.ToString(),
            entity.EmployeeId,
            oldValue,
            Snapshot(entity),
            $"Updated delegation {entity.DelegationNumber}."), cancellationToken);
        var updated = await GetDetailsAsync(entity.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    public async Task<DelegationDetailsDto> CancelAsync(Guid id, CancelDelegationRequest request, CancellationToken cancellationToken)
    {
        var entity = await GetTrackedAsync(id, cancellationToken);
        var oldValue = new { entity.Status };
        try
        {
            entity.Cancel(request.Reason, _currentUser.UserId, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new HrConflictException(exception.Message);
        }
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "DelegationCancelled",
            nameof(EmployeeDelegation),
            entity.Id.ToString(),
            entity.EmployeeId,
            oldValue,
            new { entity.Status, entity.CancellationReason, entity.CancelledAt },
            $"Cancelled delegation {entity.DelegationNumber}."), cancellationToken);
        var cancelled = await GetDetailsAsync(entity.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return cancelled;
    }

    public async Task<DelegationPrintDto> GetPrintAsync(Guid id, CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var entity = await BaseQuery().AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new HrNotFoundException("Delegation was not found.");
        return new DelegationPrintDto(
            entity.DelegationNumber,
            entity.EmployeeNameSnapshot ?? (isArabic ? entity.Employee.FullNameArabic ?? entity.Employee.FullName : entity.Employee.FullNameEnglish ?? entity.Employee.FullName),
            entity.EmployeeNumberSnapshot ?? entity.Employee.EmployeeNumber,
            entity.EmployeeNationalIdSnapshot ?? entity.Employee.NationalId,
            entity.CompanyRepresentative,
            entity.PowerOfAttorneyNumber,
            entity.PowerOfAttorneyYear,
            entity.AuthorizedEntity,
            entity.Purpose,
            entity.StartDate,
            entity.EndDate,
            entity.CreatedAt);
    }

    private IQueryable<EmployeeDelegation> BaseQuery() => _dbContext.EmployeeDelegations
        .Include(item => item.Employee).ThenInclude(employee => employee.Department)
        .Include(item => item.DelegationType)
        .Include(item => item.DelegatingEntity)
        .Include(item => item.CreatedByUser);

    private async Task<EmployeeDelegation> GetTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.EmployeeDelegations.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw new HrNotFoundException("Delegation was not found.");

    private async Task<Employee> GetActiveEmployeeAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty) throw new HrValidationException("Employee is required.");
        return await _dbContext.Employees.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken)
            ?? throw new HrValidationException("The selected employee does not exist or is inactive.");
    }

    private async Task<DelegationType> GetActiveTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty) throw new HrValidationException("Delegation type is required.");
        return await _dbContext.DelegationTypes.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken)
            ?? throw new HrValidationException("The selected delegation type does not exist or is inactive.");
    }

    private async Task<DelegationType> GetDefaultTypeAsync(CancellationToken cancellationToken) =>
        await _dbContext.DelegationTypes.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.Name).FirstOrDefaultAsync(cancellationToken)
        ?? throw new HrValidationException("No active delegation type is configured.");

    private async Task<string> ResolveEntityNameAsync(Guid? id, string suppliedName, CancellationToken cancellationToken)
    {
        if (!id.HasValue || id.Value == Guid.Empty)
        {
            if (string.IsNullOrWhiteSpace(suppliedName)) throw new HrValidationException("Bank / delegating entity is required.");
            return suppliedName.Trim();
        }
        var entity = await _dbContext.CollectionClientOrganizations.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id.Value && item.IsActive, cancellationToken)
            ?? throw new HrValidationException("The selected bank / delegating entity does not exist or is inactive.");
        return entity.NameArabic;
    }

    private static IQueryable<EmployeeDelegation> ApplySort(IQueryable<EmployeeDelegation> query, string sortBy, string direction)
    {
        var descending = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.Trim().ToLowerInvariant() switch
        {
            "number" => descending ? query.OrderByDescending(item => item.DelegationNumber) : query.OrderBy(item => item.DelegationNumber),
            "employee" => descending ? query.OrderByDescending(item => item.Employee.FullName) : query.OrderBy(item => item.Employee.FullName),
            "startdate" => descending ? query.OrderByDescending(item => item.StartDate) : query.OrderBy(item => item.StartDate),
            "enddate" => descending ? query.OrderByDescending(item => item.EndDate) : query.OrderBy(item => item.EndDate),
            "status" => descending ? query.OrderByDescending(item => item.Status) : query.OrderBy(item => item.Status),
            _ => descending ? query.OrderByDescending(item => item.CreatedAt) : query.OrderBy(item => item.CreatedAt)
        };
    }

    private static DelegationListItemDto MapList(EmployeeDelegation item, DateOnly today)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return new DelegationListItemDto(
            item.Id,
            item.DelegationNumber,
            item.EmployeeId,
            item.Employee.EmployeeNumber,
            isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
            isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
            item.DelegationTypeId,
            isArabic ? item.DelegationType.NameArabic ?? item.DelegationType.Name : item.DelegationType.Name,
            item.Subject,
            item.DelegatingEntityId,
            item.AuthorizedEntity,
            item.StartDate,
            item.EndDate,
            EffectiveStatus(item, today),
            item.CreatedByUser.FullName,
            item.CreatedAt);
    }

    private static DelegationDetailsDto MapDetails(EmployeeDelegation item, DateOnly today)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return new DelegationDetailsDto(
            item.Id,
            item.DelegationNumber,
            item.EmployeeId,
            item.Employee.EmployeeNumber,
            isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
            item.Employee.NationalId,
            isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
            item.DelegationTypeId,
            isArabic ? item.DelegationType.NameArabic ?? item.DelegationType.Name : item.DelegationType.Name,
            item.Subject,
            item.DelegatingEntityId,
            item.AuthorizedEntity,
            item.CompanyRepresentative,
            item.PowerOfAttorneyNumber,
            item.PowerOfAttorneyYear,
            item.StartDate,
            item.EndDate,
            item.Purpose,
            item.Notes,
            EffectiveStatus(item, today),
            item.CreatedByUserId,
            item.CreatedByUser.FullName,
            item.CreatedAt,
            item.UpdatedAt,
            item.CancellationReason,
            item.CancelledAt);
    }

    private static string EffectiveStatus(EmployeeDelegation item, DateOnly today) =>
        item.Status == DelegationStatuses.Active && item.EndDate < today ? DelegationStatuses.Expired : item.Status;

    private static object Snapshot(EmployeeDelegation item) => new
    {
        item.EmployeeId,
        item.DelegationTypeId,
        item.Subject,
        item.DelegatingEntityId,
        item.AuthorizedEntity,
        item.CompanyRepresentative,
        item.PowerOfAttorneyNumber,
        item.PowerOfAttorneyYear,
        item.StartDate,
        item.EndDate,
        item.Purpose,
        item.Notes,
        item.Status
    };

    private static string NormalizeStatus(string value)
    {
        try { return DelegationStatuses.Normalize(value); }
        catch (ArgumentException exception) { throw new HrValidationException(exception.Message); }
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (startDate.Year < 1900) throw new HrValidationException("Start date is required.");
        if (endDate < startDate) throw new HrValidationException("End date cannot be before start date.");
    }

    private static void ValidateEmployeeDates(Employee employee, DateOnly startDate, DateOnly endDate)
    {
        if (employee.HireDate.HasValue && startDate < employee.HireDate.Value)
            throw new HrValidationException("Delegation cannot start before the employee hire date.");
        if (employee.TerminationDate.HasValue && endDate > employee.TerminationDate.Value)
            throw new HrValidationException("Delegation cannot extend beyond the employee termination date.");
    }

    private static void ValidateFilter(DelegationFilterDto filter)
    {
        if (filter.Page < 1 || filter.PageSize is < 1 or > 200) throw new HrValidationException("Pagination values are invalid.");
        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateTo < filter.DateFrom)
            throw new HrValidationException("Date to cannot be before date from.");
        if (!string.Equals(filter.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase))
            throw new HrValidationException("Sort direction must be asc or desc.");
    }

    private static int Pages(int total, int pageSize) => total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
}
