using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface ICollectionsReportService
{
    Task<CollectionReportDto> GetExecutiveAsync(CollectionReportFilters filters, CancellationToken cancellationToken);
    Task<byte[]> ExportExecutiveCsvAsync(CollectionReportFilters filters, CancellationToken cancellationToken);
}
