using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrCalendarService
{
    Task<WorkingCalendarDto> GetWorkingCalendarAsync(CancellationToken cancellationToken);

    Task<WorkingCalendarDto> UpdateWorkingCalendarAsync(UpdateWorkingCalendarRequest request, CancellationToken cancellationToken);

    Task<PagedCalendarExceptionsDto> GetExceptionsAsync(CalendarExceptionFilterDto filter, CancellationToken cancellationToken);

    Task<CalendarExceptionDetailsDto> GetExceptionAsync(Guid exceptionId, CancellationToken cancellationToken);

    Task<CalendarExceptionDetailsDto> CreateExceptionAsync(CreateCalendarExceptionRequest request, CancellationToken cancellationToken);

    Task<CalendarExceptionDetailsDto> UpdateExceptionAsync(Guid exceptionId, UpdateCalendarExceptionRequest request, CancellationToken cancellationToken);

    Task<CalendarExceptionDetailsDto> SetExceptionActiveAsync(Guid exceptionId, bool isActive, CancellationToken cancellationToken);

    Task DeleteExceptionAsync(Guid exceptionId, DeleteCalendarExceptionRequest request, CancellationToken cancellationToken);
}
