using System.Data;
using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;

namespace MIS.Infrastructure.Services;

public sealed class HrEmployeeProfileService : IHrEmployeeProfileService
{
    private static readonly IReadOnlyDictionary<string, string> EmployeeStatuses =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [Employee.ActiveStatus] = Employee.ActiveStatus,
            [Employee.InactiveStatus] = Employee.InactiveStatus,
            [Employee.OnLeaveStatus] = Employee.OnLeaveStatus,
            [Employee.SuspendedStatus] = Employee.SuspendedStatus,
            [Employee.TerminatedStatus] = Employee.TerminatedStatus
        };

    private readonly ApplicationDbContext _dbContext;
    private readonly IHrAuditService _audit;

    public HrEmployeeProfileService(ApplicationDbContext dbContext, IHrAuditService audit)
    {
        _dbContext = dbContext;
        _audit = audit;
    }

    public async Task<EmployeeProfileDto> GetProfileAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Include(item => item.Department)
            .Include(item => item.Position)
            .Include(item => item.Branch)
            .Include(item => item.EmploymentType)
            .Include(item => item.DirectManager)
            .SingleOrDefaultAsync(item => item.Id == employeeId, cancellationToken)
            ?? throw new HrNotFoundException("Employee was not found.");

        var contract = await _dbContext.EmployeeContracts
            .AsNoTracking()
            .Include(item => item.ContractType)
            .Where(item => item.EmployeeId == employeeId)
            .OrderByDescending(item => item.Status == EmployeeContract.ActiveStatus)
            .ThenByDescending(item => item.ContractStartDate)
            .ThenByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var compensation = await _dbContext.EmployeeCompensations
            .AsNoTracking()
            .Where(item => item.EmployeeId == employeeId && item.IsCurrent)
            .OrderByDescending(item => item.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        var emergencyContact = await _dbContext.EmployeeEmergencyContacts
            .AsNoTracking()
            .Where(item => item.EmployeeId == employeeId)
            .OrderByDescending(item => item.IsPrimary)
            .ThenBy(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var documentCount = await _dbContext.EmployeeDocuments.AsNoTracking()
            .CountAsync(item => item.EmployeeId == employeeId && !item.IsDeleted, cancellationToken);
        var attendanceCount = await _dbContext.AttendanceRecords.AsNoTracking()
            .CountAsync(item => item.EmployeeId == employeeId && !item.IsDeleted, cancellationToken);
        var leaveCount = await _dbContext.LeaveRequests.AsNoTracking()
            .CountAsync(item => item.EmployeeId == employeeId, cancellationToken);
        var absenceCount = await _dbContext.EmployeeAbsences.AsNoTracking()
            .CountAsync(item => item.EmployeeId == employeeId, cancellationToken);
        var delegationCount = await _dbContext.EmployeeDelegations.AsNoTracking()
            .CountAsync(item => item.EmployeeId == employeeId, cancellationToken);

        return Map(employee, contract, compensation, emergencyContact, documentCount, attendanceCount, leaveCount, absenceCount, delegationCount);
    }

    public async Task<EmployeeReportingLineDto> GetReportingLineAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var employee = await _dbContext.Employees.AsNoTracking()
            .Include(item => item.DirectManager)
            .SingleOrDefaultAsync(item => item.Id == employeeId, cancellationToken)
            ?? throw new HrNotFoundException("Employee was not found.");

        var reports = await _dbContext.Employees.AsNoTracking()
            .Where(item => item.DirectManagerId == employeeId)
            .OrderBy(item => item.FullName)
            .Select(item => new ReportingLineEmployeeDto(
                item.Id,
                item.EmployeeNumber,
                isArabic ? item.FullNameArabic ?? item.FullName : item.FullNameEnglish ?? item.FullName,
                item.Status))
            .ToArrayAsync(cancellationToken);

        return new EmployeeReportingLineDto(
            employee.Id,
            GetDisplayName(employee),
            employee.DirectManagerId,
            employee.DirectManager is null ? null : GetDisplayName(employee.DirectManager),
            reports);
    }

    public async Task<EmployeeProfileDto> UpdatePersonalAsync(
        Guid employeeId,
        UpdateEmployeePersonalRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await GetTrackedEmployeeAsync(employeeId, cancellationToken);
        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow) || request.DateOfBirth < new DateOnly(1900, 1, 1))
        {
            throw new HrValidationException("Date of birth must be a valid past date.");
        }

        var normalizedNationalId = Normalize(request.NationalId);
        if (normalizedNationalId is not null && await _dbContext.Employees.AnyAsync(
                item => item.Id != employeeId && item.NationalId == normalizedNationalId,
                cancellationToken))
        {
            throw new HrConflictException("Another employee already uses this national ID.");
        }

        var oldValue = new
        {
            employee.FullNameArabic,
            employee.FullNameEnglish,
            employee.NationalId,
            employee.DateOfBirth,
            employee.Gender,
            employee.MaritalStatus
        };
        var now = DateTimeOffset.UtcNow;
        employee.UpdatePersonalInformation(
            request.FullNameArabic,
            request.FullNameEnglish,
            normalizedNationalId,
            request.DateOfBirth,
            request.Gender,
            request.MaritalStatus,
            employee.ProfilePhotoStorageKey,
            now);

        var canonicalName = Normalize(request.FullNameEnglish) ?? Normalize(request.FullNameArabic) ?? employee.FullName;
        employee.Update(employee.EmployeeNumber, canonicalName, employee.DepartmentId, employee.IsActive, now);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditEmployeeAsync("EmployeePersonalUpdated", employee, oldValue, request, "Updated personal information.", cancellationToken);
        var profile = await GetProfileAsync(employeeId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<EmployeeProfileDto> UpdateContactAsync(
        Guid employeeId,
        UpdateEmployeeContactRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await GetTrackedEmployeeAsync(employeeId, cancellationToken);
        var oldValue = new
        {
            employee.MobileNumber,
            employee.AlternativeMobileNumber,
            employee.Email,
            employee.Address,
            employee.City
        };

        employee.UpdateContactInformation(
            request.MobileNumber,
            request.AlternativeMobileNumber,
            request.Email,
            request.Address,
            request.City,
            DateTimeOffset.UtcNow);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditEmployeeAsync("EmployeeContactUpdated", employee, oldValue, request, "Updated contact information.", cancellationToken);
        var profile = await GetProfileAsync(employeeId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<EmployeeProfileDto> UpdateEmploymentAsync(
        Guid employeeId,
        UpdateEmployeeEmploymentRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await GetTrackedEmployeeAsync(employeeId, cancellationToken);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await ValidateEmploymentLookupsAsync(employeeId, request, cancellationToken);
        var oldValue = await GetEmploymentAuditSnapshotAsync(employeeId, cancellationToken);

        employee.UpdateEmploymentInformation(
            request.DepartmentId,
            request.PositionId,
            request.BranchId,
            request.EmploymentTypeId,
            request.DirectManagerId,
            request.HireDate,
            DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var newValue = await GetEmploymentAuditSnapshotAsync(employeeId, cancellationToken);
        await AuditEmployeeAsync("EmployeeEmploymentUpdated", employee, oldValue, newValue, "Updated employment information.", cancellationToken);
        var profile = await GetProfileAsync(employeeId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<EmployeeProfileDto> UpdateContractAsync(
        Guid employeeId,
        UpdateEmployeeContractRequest request,
        CancellationToken cancellationToken)
    {
        _ = await GetTrackedEmployeeAsync(employeeId, cancellationToken);
        if (!request.ContractTypeId.HasValue || request.ContractTypeId == Guid.Empty || !request.StartDate.HasValue)
        {
            throw new HrValidationException("Contract type and start date are required.");
        }

        if (!await _dbContext.ContractTypes.AnyAsync(
                item => item.Id == request.ContractTypeId && item.IsActive,
                cancellationToken))
        {
            throw new HrValidationException("The selected contract type does not exist or is inactive.");
        }

        ValidateDateRange(request.StartDate, request.EndDate, "Contract end date cannot be before its start date.");
        ValidateDateRange(request.ProbationStartDate, request.ProbationEndDate, "Probation end date cannot be before its start date.");
        if (!EmployeeContract.IsValidStatus(request.Status))
        {
            throw new HrValidationException("Contract status must be Draft, Active, Expired, or Terminated.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var previousContract = await _dbContext.EmployeeContracts
            .Where(item => item.EmployeeId == employeeId)
            .OrderByDescending(item => item.Status == EmployeeContract.ActiveStatus)
            .ThenByDescending(item => item.ContractStartDate)
            .ThenByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var oldValue = previousContract is null ? null : new
        {
            previousContract.Id,
            previousContract.ContractTypeId,
            StartDate = previousContract.ContractStartDate,
            EndDate = previousContract.ContractEndDate,
            previousContract.ProbationStartDate,
            previousContract.ProbationEndDate,
            previousContract.Status,
            previousContract.Notes
        };
        var now = DateTimeOffset.UtcNow;

        if (previousContract?.Status is EmployeeContract.ActiveStatus or EmployeeContract.DraftStatus)
        {
            if (request.StartDate.Value < previousContract.ContractStartDate)
                throw new HrConflictException("A replacement contract cannot start before the current contract version.");
            previousContract.CloseForReplacement(request.StartDate.Value, now);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var contract = new EmployeeContract(
            employeeId,
            request.ContractTypeId.Value,
            request.StartDate.Value,
            request.EndDate,
            request.ProbationStartDate,
            request.ProbationEndDate,
            request.Status,
            request.Notes,
            now);
        _dbContext.EmployeeContracts.Add(contract);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "EmployeeContractUpdated",
            nameof(EmployeeContract),
            contract.Id.ToString(),
            employeeId,
            oldValue,
            new
            {
                contract.Id,
                contract.ContractTypeId,
                StartDate = contract.ContractStartDate,
                EndDate = contract.ContractEndDate,
                contract.ProbationStartDate,
                contract.ProbationEndDate,
                contract.Status,
                contract.Notes
            },
            previousContract is null ? "Created employee contract information." : "Replaced employee contract information while preserving the previous version."), cancellationToken);
        var profile = await GetProfileAsync(employeeId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<EmployeeProfileDto> UpdateCompensationAsync(
        Guid employeeId,
        UpdateEmployeeCompensationRequest request,
        CancellationToken cancellationToken)
    {
        _ = await GetTrackedEmployeeAsync(employeeId, cancellationToken);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var previousCompensation = await _dbContext.EmployeeCompensations
            .Where(item => item.EmployeeId == employeeId && item.IsCurrent)
            .OrderByDescending(item => item.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
        var oldValue = previousCompensation is null ? null : CompensationAuditMetadata(previousCompensation);
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (previousCompensation is not null)
        {
            if (previousCompensation.EffectiveFrom > today)
                throw new HrConflictException("The current compensation is future-dated and must be corrected before adding a new version.");
            var effectiveTo = previousCompensation.EffectiveFrom < today ? today.AddDays(-1) : today;
            previousCompensation.Close(effectiveTo, now);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var compensation = new EmployeeCompensation(
            employeeId,
            request.BasicSalary,
            request.Allowances,
            today,
            null,
            true,
            request.BankName,
            request.BankAccount,
            request.Iban,
            request.Notes,
            now);
        _dbContext.EmployeeCompensations.Add(compensation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var changedFields = GetCompensationChangedFields(previousCompensation, compensation);
        await _audit.WriteAsync(new AuditWriteRequest(
            "EmployeeCompensationUpdated",
            nameof(EmployeeCompensation),
            compensation.Id.ToString(),
            employeeId,
            oldValue,
            new
            {
                VersionId = compensation.Id,
                compensation.EffectiveFrom,
                compensation.EffectiveTo,
                compensation.IsCurrent,
                ChangeKind = previousCompensation is null ? "Created" : "Replaced",
                ChangedFields = changedFields
            },
            previousCompensation is null
                ? "Created restricted employee compensation information. The audit contains metadata only."
                : "Replaced restricted employee compensation information while preserving history. The audit contains metadata only."), cancellationToken);
        var profile = await GetProfileAsync(employeeId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<EmployeeProfileDto> UpdateEmergencyContactAsync(
        Guid employeeId,
        UpdateEmployeeEmergencyContactRequest request,
        CancellationToken cancellationToken)
    {
        _ = await GetTrackedEmployeeAsync(employeeId, cancellationToken);
        var contact = await _dbContext.EmployeeEmergencyContacts
            .Where(item => item.EmployeeId == employeeId && item.IsPrimary)
            .OrderBy(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var oldValue = contact is null ? null : new
        {
            contact.ContactName,
            contact.Relationship,
            contact.MobileNumber,
            contact.AlternativeNumber,
            contact.Notes
        };
        var now = DateTimeOffset.UtcNow;

        if (contact is null)
        {
            contact = new EmployeeEmergencyContact(
                employeeId,
                request.ContactName,
                request.Relationship,
                request.MobileNumber,
                request.AlternativeNumber,
                request.Notes,
                true,
                now);
            _dbContext.EmployeeEmergencyContacts.Add(contact);
        }
        else
        {
            contact.Update(
                request.ContactName,
                request.Relationship,
                request.MobileNumber,
                request.AlternativeNumber,
                request.Notes,
                true,
                now);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "EmployeeEmergencyContactUpdated",
            nameof(EmployeeEmergencyContact),
            contact.Id.ToString(),
            employeeId,
            oldValue,
            request,
            "Updated emergency contact information."), cancellationToken);
        var profile = await GetProfileAsync(employeeId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<EmployeeProfileDto> ChangeStatusAsync(
        Guid employeeId,
        ChangeEmployeeStatusRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await GetTrackedEmployeeAsync(employeeId, cancellationToken);
        if (!EmployeeStatuses.TryGetValue(request.Status.Trim(), out var status))
        {
            throw new HrValidationException("Employee status must be Active, Inactive, OnLeave, Suspended, or Terminated.");
        }
        if (status == Employee.TerminatedStatus)
        {
            var terminationDate = request.TerminationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            if (terminationDate > DateOnly.FromDateTime(DateTime.UtcNow) || employee.HireDate > terminationDate)
                throw new HrValidationException("Termination date cannot be in the future or before the employee hire date.");
        }

        var oldValue = new { employee.Status, employee.IsActive, employee.TerminationDate, employee.TerminationReason };
        var isActive = status is Employee.ActiveStatus or Employee.OnLeaveStatus;
        employee.ChangeStatus(
            status,
            isActive,
            status == Employee.TerminatedStatus ? request.TerminationDate ?? DateOnly.FromDateTime(DateTime.UtcNow) : null,
            status == Employee.TerminatedStatus ? request.Reason : null,
            DateTimeOffset.UtcNow);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditEmployeeAsync("EmployeeStatusChanged", employee, oldValue, new { Status = status, request.Reason, employee.TerminationDate }, "Changed employee status.", cancellationToken);
        var profile = await GetProfileAsync(employeeId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    private async Task<Employee> GetTrackedEmployeeAsync(Guid employeeId, CancellationToken cancellationToken) =>
        await _dbContext.Employees.SingleOrDefaultAsync(item => item.Id == employeeId, cancellationToken)
        ?? throw new HrNotFoundException("Employee was not found.");

    private async Task ValidateEmploymentLookupsAsync(
        Guid employeeId,
        UpdateEmployeeEmploymentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DepartmentId == Guid.Empty || !await _dbContext.Departments.AnyAsync(
                item => item.Id == request.DepartmentId && item.IsActive,
                cancellationToken))
        {
            throw new HrValidationException("The selected department does not exist or is inactive.");
        }

        if (request.PositionId.HasValue && !await _dbContext.Positions.AnyAsync(
                item => item.Id == request.PositionId && item.IsActive &&
                        (!item.DepartmentId.HasValue || item.DepartmentId == request.DepartmentId),
                cancellationToken))
            throw new HrValidationException("The selected position does not exist, is inactive, or belongs to another department.");
        if (request.BranchId.HasValue && !await _dbContext.Branches.AnyAsync(item => item.Id == request.BranchId && item.IsActive, cancellationToken))
            throw new HrValidationException("The selected branch does not exist or is inactive.");
        if (request.EmploymentTypeId.HasValue && !await _dbContext.EmploymentTypes.AnyAsync(item => item.Id == request.EmploymentTypeId && item.IsActive, cancellationToken))
            throw new HrValidationException("The selected employment type does not exist or is inactive.");

        if (!request.DirectManagerId.HasValue) return;
        if (request.DirectManagerId == employeeId) throw new HrValidationException("An employee cannot be their own direct manager.");

        var managerId = request.DirectManagerId;
        var visited = new HashSet<Guid>();
        var isDirectManager = true;
        while (managerId.HasValue)
        {
            if (!visited.Add(managerId.Value) || managerId == employeeId)
                throw new HrValidationException("The selected direct manager would create a reporting-line cycle.");

            var manager = await _dbContext.Employees.AsNoTracking()
                .Where(item => item.Id == managerId && (!isDirectManager || item.IsActive))
                .Select(item => new { item.Id, item.DirectManagerId })
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new HrValidationException("The selected direct manager does not exist or is inactive.");
            managerId = manager.DirectManagerId;
            isDirectManager = false;
        }
    }

    private Task AuditEmployeeAsync(
        string action,
        Employee employee,
        object? oldValue,
        object? newValue,
        string description,
        CancellationToken cancellationToken) =>
        _audit.WriteAsync(new AuditWriteRequest(
            action,
            nameof(Employee),
            employee.Id.ToString(),
            employee.Id,
            oldValue,
            newValue,
            description), cancellationToken);

    private Task<EmploymentAuditSnapshot> GetEmploymentAuditSnapshotAsync(
        Guid employeeId,
        CancellationToken cancellationToken) =>
        _dbContext.Employees.AsNoTracking()
            .Where(item => item.Id == employeeId)
            .Select(item => new EmploymentAuditSnapshot(
                item.DepartmentId,
                item.Department.Name,
                item.PositionId,
                item.Position == null ? null : item.Position.Name,
                item.BranchId,
                item.Branch == null ? null : item.Branch.Name,
                item.EmploymentTypeId,
                item.EmploymentType == null ? null : item.EmploymentType.Name,
                item.DirectManagerId,
                item.DirectManager == null
                    ? null
                    : item.DirectManager.FullNameEnglish ?? item.DirectManager.FullNameArabic ?? item.DirectManager.FullName,
                item.HireDate))
            .SingleAsync(cancellationToken);

    private static object CompensationAuditMetadata(EmployeeCompensation compensation) => new
    {
        VersionId = compensation.Id,
        compensation.EffectiveFrom,
        compensation.EffectiveTo,
        compensation.IsCurrent
    };

    private static string[] GetCompensationChangedFields(
        EmployeeCompensation? previous,
        EmployeeCompensation current)
    {
        var changedFields = new List<string>();

        if (previous is null ||
            previous.BasicSalary != current.BasicSalary ||
            previous.Allowances != current.Allowances)
        {
            changedFields.Add("CompensationAmounts");
        }

        if (previous is null ||
            !string.Equals(previous.BankName, current.BankName, StringComparison.Ordinal) ||
            !string.Equals(previous.BankAccountNumber, current.BankAccountNumber, StringComparison.Ordinal) ||
            !string.Equals(previous.Iban, current.Iban, StringComparison.Ordinal))
        {
            changedFields.Add("BankingDetails");
        }

        if (previous is null || !string.Equals(previous.Notes, current.Notes, StringComparison.Ordinal))
        {
            changedFields.Add("Notes");
        }

        return changedFields.ToArray();
    }

    private static EmployeeProfileDto Map(
        Employee employee,
        EmployeeContract? contract,
        EmployeeCompensation? compensation,
        EmployeeEmergencyContact? emergencyContact,
        int documentCount,
        int attendanceCount,
        int leaveCount,
        int absenceCount,
        int delegationCount)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return new EmployeeProfileDto(
        employee.Id,
        employee.EmployeeNumber,
        GetDisplayName(employee),
        employee.Status,
        employee.IsActive,
        !string.IsNullOrWhiteSpace(employee.ProfilePhotoStorageKey),
        new EmployeePersonalInformationDto(
            employee.FullNameArabic,
            employee.FullNameEnglish,
            employee.NationalId,
            employee.DateOfBirth,
            employee.Gender,
            employee.MaritalStatus),
        new EmployeeContactInformationDto(
            employee.MobileNumber,
            employee.AlternativeMobileNumber,
            employee.Email,
            employee.Address,
            employee.City),
        new EmployeeEmploymentInformationDto(
            employee.DepartmentId,
            isArabic ? employee.Department.NameArabic ?? employee.Department.Name : employee.Department.Name,
            employee.Department.Code,
            employee.PositionId,
            employee.Position is null ? null : isArabic ? employee.Position.NameArabic ?? employee.Position.Name : employee.Position.Name,
            employee.BranchId,
            employee.Branch is null ? null : isArabic ? employee.Branch.NameArabic ?? employee.Branch.Name : employee.Branch.Name,
            employee.EmploymentTypeId,
            employee.EmploymentType is null ? null : isArabic ? employee.EmploymentType.NameArabic ?? employee.EmploymentType.Name : employee.EmploymentType.Name,
            employee.DirectManagerId,
            employee.DirectManager is null ? null : GetDisplayName(employee.DirectManager),
            employee.HireDate,
            employee.Status),
        contract is null ? null : new EmployeeContractInformationDto(
            contract.Id,
            contract.ContractTypeId,
            isArabic ? contract.ContractType.NameArabic ?? contract.ContractType.Name : contract.ContractType.Name,
            contract.ContractStartDate,
            contract.ContractEndDate,
            contract.ProbationStartDate,
            contract.ProbationEndDate,
            contract.Status,
            contract.Notes,
            contract.UpdatedAt ?? contract.CreatedAt),
        compensation is null ? null : new EmployeeCompensationDto(
            compensation.Id,
            compensation.BasicSalary,
            compensation.Allowances,
            compensation.TotalSalary,
            compensation.BankName,
            compensation.BankAccountNumber,
            compensation.Iban,
            compensation.Notes,
            compensation.UpdatedAt ?? compensation.CreatedAt),
        emergencyContact is null ? null : new EmployeeEmergencyContactDto(
            emergencyContact.Id,
            emergencyContact.ContactName,
            emergencyContact.Relationship,
            emergencyContact.MobileNumber,
            emergencyContact.AlternativeNumber,
            emergencyContact.Notes,
            emergencyContact.UpdatedAt ?? emergencyContact.CreatedAt),
        new EmployeeProfileCountersDto(documentCount, attendanceCount, leaveCount, absenceCount, delegationCount),
        employee.CreatedAt,
        employee.UpdatedAt);
    }

    private static string GetDisplayName(Employee employee) =>
        ApiTextLocalizer.IsArabic
            ? employee.FullNameArabic ?? employee.FullName
            : employee.FullNameEnglish ?? employee.FullName;

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateDateRange(DateOnly? start, DateOnly? end, string message)
    {
        if (start.HasValue && end.HasValue && end < start) throw new HrValidationException(message);
    }

    private sealed record EmploymentAuditSnapshot(
        Guid DepartmentId,
        string Department,
        Guid? PositionId,
        string? Position,
        Guid? BranchId,
        string? Branch,
        Guid? EmploymentTypeId,
        string? EmploymentType,
        Guid? DirectManagerId,
        string? DirectManager,
        DateOnly? HireDate);
}
