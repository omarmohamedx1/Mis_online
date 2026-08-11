using System.Globalization;
using MIS.Application.Common;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class ApiTextLocalizerTests
{
    [Fact]
    public void English_request_preserves_canonical_message_and_codes()
    {
        using var culture = new CultureScope("en");

        Assert.Equal("Employee was not found.", ApiTextLocalizer.Localize("Employee was not found.", true));
        Assert.Equal("Active", ApiTextLocalizer.LocalizeCode("Active"));
    }

    [Fact]
    public void Arabic_request_localizes_errors_and_validation_details()
    {
        using var culture = new CultureScope("ar");

        var response = ApiErrorResponse.Failure(
            "Validation failed.",
            ["The EmployeeId field is required.", "Attendance already exists for this employee and date."]);

        Assert.Equal("تعذر التحقق من صحة البيانات.", response.Message);
        Assert.Contains("حقل الموظف مطلوب.", response.Errors);
        Assert.Contains("يوجد بالفعل سجل حضور لهذا الموظف في هذا التاريخ.", response.Errors);
    }

    [Fact]
    public void Arabic_request_localizes_dynamic_mixed_language_audit_text()
    {
        using var culture = new CultureScope("ar");

        Assert.Equal("تم إنشاء الموظف أحمد علي.", ApiTextLocalizer.Localize("Created employee أحمد علي."));
        Assert.Equal("تم إنشاء الإدارة ضمن الفروع.", ApiTextLocalizer.Localize("Created الإدارة in branches."));
    }

    [Fact]
    public void Arabic_unknown_error_uses_safe_arabic_fallback()
    {
        using var culture = new CultureScope("ar");

        var localized = ApiTextLocalizer.Localize("A new unmapped technical failure.", true);

        Assert.Equal("تعذر إكمال الطلب. يُرجى مراجعة البيانات والمحاولة مرة أخرى.", localized);
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _previousUiCulture = CultureInfo.CurrentUICulture;

        public CultureScope(string cultureName)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }
}
