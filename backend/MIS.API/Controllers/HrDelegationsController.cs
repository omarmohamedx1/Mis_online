using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/delegations")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrDelegationsController : ControllerBase
{
    private readonly IHrDelegationService _service;
    private readonly IConfiguration _configuration;

    public HrDelegationsController(IHrDelegationService service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<PagedDelegationsDto>> GetPaged(
        [FromQuery] DelegationFilterDto filter,
        CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(filter, cancellationToken));

    [HttpGet("entities")]
    public async Task<ActionResult<IReadOnlyCollection<DelegationEntityOptionDto>>> GetEntities(CancellationToken cancellationToken)
        => Ok(await _service.GetEntitiesAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DelegationDetailsDto>> GetDetails(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetDetailsAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<DelegationDetailsDto>> Create(
        CreateDelegationRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDetails), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DelegationDetailsDto>> Update(
        Guid id,
        UpdateDelegationRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<DelegationDetailsDto>> Cancel(
        Guid id,
        CancelDelegationRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.CancelAsync(id, request, cancellationToken));

    [HttpGet("{id:guid}/print")]
    [Produces("text/html")]
    public async Task<ContentResult> Print(Guid id, CancellationToken cancellationToken)
    {
        var delegation = await _service.GetPrintAsync(id, cancellationToken);
        Response.Headers.CacheControl = "no-store, private";
        Response.Headers.Pragma = "no-cache";
        return Content(BuildPrintableHtml(delegation), "text/html; charset=utf-8");
    }

    private string BuildPrintableHtml(DelegationPrintDto item)
    {
        string Encode(string value) => HtmlEncoder.Default.Encode(value);
        var isArabic = ApiTextLocalizer.IsArabic;
        var culture = isArabic ? CultureInfo.GetCultureInfo("ar-EG") : CultureInfo.GetCultureInfo("en-GB");
        var language = isArabic ? "ar" : "en";
        var direction = isArabic ? "rtl" : "ltr";
        var companyEnglish = Encode(_configuration["Company:NameEnglish"] ?? "MIS Company");
        var companyArabic = Encode(_configuration["Company:NameArabic"] ?? "شركة MIS");
        var companyName = isArabic ? companyArabic : companyEnglish;
        var address = Encode(_configuration["Company:Address"] ?? string.Empty);
        var registration = Encode(_configuration["Company:RegistrationNumber"] ?? string.Empty);
        var purpose = Encode(item.Purpose).Replace("\r\n", "<br>", StringComparison.Ordinal).Replace("\n", "<br>", StringComparison.Ordinal);
        var nationalId = string.IsNullOrWhiteSpace(item.NationalId) ? "—" : Encode(item.NationalId);
        var authorizedEntity = string.IsNullOrWhiteSpace(item.AuthorizedEntity) ? "—" : Encode(item.AuthorizedEntity);
        var createdDate = Encode(item.CreatedAt.ToString("dd MMM yyyy", culture));
        var startDate = Encode(item.StartDate.ToString("dd MMM yyyy", culture));
        var endDate = Encode(item.EndDate.ToString("dd MMM yyyy", culture));
        var documentTitle = isArabic ? "تفويض إداري رسمي" : "Official Administrative Delegation";
        var statement = isArabic
            ? "تفوض الشركة بموجب هذا المستند الموظف الموضحة بياناته أدناه للقيام بالغرض والإجراء المفوض به خلال مدة سريان التفويض."
            : "The company hereby delegates the employee identified below to perform the stated purpose and authorized action during the effective delegation period.";
        var delegationNumberLabel = isArabic ? "رقم التفويض" : "Delegation No.";
        var createdLabel = isArabic ? "تاريخ الإنشاء" : "Created";
        var registrationLabel = isArabic ? "رقم التسجيل" : "Registration";
        var employeeLabel = isArabic ? "اسم الموظف" : "Employee";
        var employeeNumberLabel = isArabic ? "الرقم الوظيفي" : "Employee ID";
        var nationalIdLabel = isArabic ? "الرقم القومي" : "National ID";
        var typeLabel = isArabic ? "يمثلها السيد الأستاذ" : "Company Representative";
        var subjectLabel = isArabic ? "رقم التوكيل / السنة" : "Power of Attorney No. / Year";
        var entityLabel = isArabic ? "الجهة المفوض إليها" : "Authorized Entity";
        var startLabel = isArabic ? "تاريخ البداية" : "Start Date";
        var endLabel = isArabic ? "تاريخ النهاية" : "End Date";
        var actionLabel = isArabic ? "الغرض والإجراء المفوض" : "Purpose and Authorized Action";
        var employeeSignature = isArabic ? "توقيع الموظف" : "Employee Signature";
        var authorizedSignature = isArabic ? "المفوض بالتوقيع" : "Authorized Signatory";
        var companyStamp = isArabic ? "ختم الشركة" : "Company Stamp";
        return $$"""
        <!doctype html>
        <html lang="{{language}}" dir="{{direction}}">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>{{Encode(item.DelegationNumber)}} - {{documentTitle}}</title>
          <style>
            @page { size: A4; margin: 18mm; }
            * { box-sizing: border-box; }
            body { margin: 0; color: #102a43; font-family: Tahoma, Arial, sans-serif; background: #eef3f8; }
            .sheet { width: 210mm; min-height: 297mm; margin: 12px auto; padding: 18mm; background: white; border-top: 6px solid #0f6f9f; }
            header { display: flex; justify-content: space-between; gap: 24px; align-items: start; border-bottom: 1px solid #c9d8e6; padding-bottom: 18px; }
            .company h1 { margin: 0 0 5px; font-size: 22px; color: #073763; }
            .company p, .meta p { margin: 3px 0; color: #52697d; font-size: 12px; }
            .meta { text-align: start; }
            h2 { text-align: center; margin: 32px 0 26px; font-size: 25px; color: #073763; }
            table { width: 100%; border-collapse: collapse; margin: 18px 0; }
            th, td { padding: 11px 12px; border: 1px solid #c9d8e6; text-align: start; vertical-align: top; font-size: 13px; }
            th { width: 28%; background: #f1f7fb; color: #073763; }
            .statement { margin: 24px 0; line-height: 2; font-size: 15px; }
            .signatures { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 22px; margin-top: 65px; text-align: center; }
            .signature { min-height: 90px; border-top: 1px solid #52697d; padding-top: 9px; font-size: 13px; }
            .stamp { height: 74px; border: 1px dashed #829ab1; border-radius: 50%; display: grid; place-items: center; color: #829ab1; margin: 14px auto 0; width: 74px; font-size: 11px; }
            footer { margin-top: 46px; padding-top: 12px; border-top: 1px solid #d9e2ec; text-align: center; color: #829ab1; font-size: 10px; }
            @media print { body { background: white; } .sheet { margin: 0; padding: 0; border-top-width: 5px; width: auto; min-height: auto; } }
          </style>
        </head>
        <body>
          <main class="sheet">
            <header>
              <div class="company"><h1>{{companyName}}</h1><p>{{address}}</p></div>
              <div class="meta"><p><strong>{{delegationNumberLabel}}</strong> {{Encode(item.DelegationNumber)}}</p><p><strong>{{createdLabel}}</strong> {{createdDate}}</p><p><strong>{{registrationLabel}}</strong> {{registration}}</p></div>
            </header>
            <h2>{{documentTitle}}</h2>
            <p class="statement">{{statement}}</p>
            <table>
              <tr><th>{{employeeLabel}}</th><td>{{Encode(item.EmployeeName)}}</td></tr>
              <tr><th>{{employeeNumberLabel}}</th><td>{{Encode(item.EmployeeNumber)}}</td></tr>
              <tr><th>{{nationalIdLabel}}</th><td>{{nationalId}}</td></tr>
              <tr><th>{{typeLabel}}</th><td>{{Encode(item.CompanyRepresentative ?? "—")}}</td></tr>
              <tr><th>{{subjectLabel}}</th><td>{{Encode(item.PowerOfAttorneyNumber ?? "—")}} / {{item.PowerOfAttorneyYear?.ToString(culture) ?? "—"}}</td></tr>
              <tr><th>{{entityLabel}}</th><td>{{authorizedEntity}}</td></tr>
              <tr><th>{{startLabel}}</th><td>{{startDate}}</td></tr>
              <tr><th>{{endLabel}}</th><td>{{endDate}}</td></tr>
              <tr><th>{{actionLabel}}</th><td>{{purpose}}</td></tr>
            </table>
            <div class="signatures">
              <div class="signature">{{employeeSignature}}</div>
              <div class="signature">{{authorizedSignature}}</div>
              <div class="signature">{{companyStamp}}<div class="stamp">{{companyStamp}}</div></div>
            </div>
            <footer>{{companyName}} · {{Encode(item.DelegationNumber)}}</footer>
          </main>
        </body>
        </html>
        """;
    }
}
