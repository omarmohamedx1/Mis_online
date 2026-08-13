using ClosedXML.Excel;
using MIS.Application.DTOs.Hr;
using MIS.Infrastructure.Services;
using PdfSharp.Pdf.IO;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class HrReportExportWriterTests
{
    [Fact]
    public void Excel_and_pdf_exports_include_the_embedded_company_branding()
    {
        HrReportColumnDto[] columns =
        [
            new("employeeNumber", "Employee ID"),
            new("employeeName", "Employee Name")
        ];
        HrReportRowDto[] rows =
        [
            new(new Dictionary<string, string?>
            {
                ["employeeNumber"] = "EMP-001",
                ["employeeName"] = "Test Employee"
            })
        ];

        var excel = HrReportExportWriter.WriteExcel(
            "Employee List", columns, rows, new Dictionary<string, string>(), DateTimeOffset.UtcNow);
        using var workbook = new XLWorkbook(new MemoryStream(excel));
        Assert.Single(workbook.Worksheet(1).Pictures);

        var pdf = HrReportExportWriter.WritePdf(
            "Employee List", columns, rows, new Dictionary<string, string>(), DateTimeOffset.UtcNow);
        using var document = PdfReader.Open(new MemoryStream(pdf), PdfDocumentOpenMode.Import);
        Assert.True(document.PageCount >= 1);
        Assert.True(pdf.Length > 10_000);
    }

    [Fact]
    public void Pdf_splits_wide_reports_into_readable_column_groups()
    {
        var columns = Enumerable.Range(1, 18)
            .Select(index => new HrReportColumnDto($"field{index}", $"Business Field {index}"))
            .ToArray();
        var row = new HrReportRowDto(columns.ToDictionary(
            column => column.Key,
            column => (string?)$"Readable value for {column.Header}"));

        var pdf = HrReportExportWriter.WritePdf(
            "Wide Employee Details", columns, [row], new Dictionary<string, string>(), DateTimeOffset.UtcNow);

        using var document = PdfReader.Open(new MemoryStream(pdf), PdfDocumentOpenMode.Import);
        Assert.True(document.PageCount >= 4);
    }
}
