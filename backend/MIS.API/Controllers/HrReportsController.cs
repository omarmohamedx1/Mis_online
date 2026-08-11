using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/reports")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrReportsController : ControllerBase
{
    private readonly IHrReportService _service;

    public HrReportsController(IHrReportService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<HrReportCatalogItemDto>> GetCatalog() =>
        Ok(_service.GetCatalog());

    [HttpGet("{reportCode}/preview")]
    public async Task<ActionResult<HrReportPreviewDto>> GetPreview(
        string reportCode,
        [FromQuery] HrReportFilterDto filter,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetPreviewAsync(reportCode, filter, cancellationToken));

    [HttpGet("{reportCode}/export")]
    public async Task<IActionResult> Export(
        string reportCode,
        [FromQuery] string format,
        [FromQuery] HrReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var report = await _service.ExportAsync(reportCode, format, filter, cancellationToken);
        return File(report.Content, report.ContentType, report.FileName);
    }
}
