using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrAttendanceService
{
    Task<PagedAttendanceRecordsDto> GetPagedAsync(AttendanceFilterDto filter, CancellationToken cancellationToken);

    Task<AttendanceDetailsDto> GetDetailsAsync(Guid attendanceId, CancellationToken cancellationToken);

    Task<AttendanceDetailsDto> CreateManualAsync(CreateManualAttendanceRequest request, CancellationToken cancellationToken);

    Task<AttendanceDetailsDto> UpdateManualAsync(Guid attendanceId, UpdateManualAttendanceRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid attendanceId, DeleteAttendanceRequest request, CancellationToken cancellationToken);

    Task<ProcessAttendanceDayResultDto> ProcessDayAsync(ProcessAttendanceDayRequest request, CancellationToken cancellationToken);
}
