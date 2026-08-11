using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrReportService
{
    IReadOnlyCollection<HrReportCatalogItemDto> GetCatalog();

    Task<HrReportPreviewDto> GetPreviewAsync(
        string reportCode,
        HrReportFilterDto filter,
        CancellationToken cancellationToken);

    Task<HrReportFileDto> ExportAsync(
        string reportCode,
        string format,
        HrReportFilterDto filter,
        CancellationToken cancellationToken);
}
