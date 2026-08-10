using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrDashboardRepository
{
    Task<HrDashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
}
