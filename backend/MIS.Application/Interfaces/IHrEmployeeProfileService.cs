using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrEmployeeProfileService
{
    Task<EmployeeProfileDto> GetProfileAsync(Guid employeeId, CancellationToken cancellationToken);

    Task<EmployeeReportingLineDto> GetReportingLineAsync(Guid employeeId, CancellationToken cancellationToken);

    Task<EmployeeProfileDto> UpdatePersonalAsync(Guid employeeId, UpdateEmployeePersonalRequest request, CancellationToken cancellationToken);

    Task<EmployeeProfileDto> UpdateContactAsync(Guid employeeId, UpdateEmployeeContactRequest request, CancellationToken cancellationToken);

    Task<EmployeeProfileDto> UpdateEmploymentAsync(Guid employeeId, UpdateEmployeeEmploymentRequest request, CancellationToken cancellationToken);

    Task<EmployeeProfileDto> UpdateContractAsync(Guid employeeId, UpdateEmployeeContractRequest request, CancellationToken cancellationToken);

    Task<EmployeeProfileDto> UpdateCompensationAsync(Guid employeeId, UpdateEmployeeCompensationRequest request, CancellationToken cancellationToken);

    Task<EmployeeProfileDto> UpdateEmergencyContactAsync(Guid employeeId, UpdateEmployeeEmergencyContactRequest request, CancellationToken cancellationToken);

    Task<EmployeeProfileDto> ChangeStatusAsync(Guid employeeId, ChangeEmployeeStatusRequest request, CancellationToken cancellationToken);
}
