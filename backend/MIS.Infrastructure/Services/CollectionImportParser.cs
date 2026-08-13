using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using MIS.Application.Common;

namespace MIS.Infrastructure.Services;

internal sealed record ParsedCollectionRow(int RowNumber, IReadOnlyDictionary<string, string> Values);

internal static class CollectionImportParser
{
    public const int MaximumRows = 20_000;
    public const int MaximumColumns = 40;
    static CollectionImportParser() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static async Task<IReadOnlyCollection<ParsedCollectionRow>> ParseAsync(Stream stream, string extension, CancellationToken token)
    {
        if (!stream.CanSeek) throw new HrValidationException("The stored import file must be seekable."); stream.Position = 0;
        return extension switch { ".csv" => await ParseCsvAsync(stream, token), ".xlsx" => ParseWorkbook(stream, token), _ => throw new HrValidationException("Only CSV and XLSX collection imports are supported.") };
    }

    private static async Task<IReadOnlyCollection<ParsedCollectionRow>> ParseCsvAsync(Stream stream, CancellationToken token)
    {
        var prefix = new byte[Math.Min(4096, (int)stream.Length)]; _ = await stream.ReadAsync(prefix, token); if (prefix.Any(x => x == 0)) throw new HrValidationException("The CSV file contains binary data."); stream.Position = 0;
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true, leaveOpen: true); using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { BadDataFound = _ => throw new HrValidationException("The CSV file contains malformed data."), MissingFieldFound = null, TrimOptions = TrimOptions.Trim });
        if (!await csv.ReadAsync() || !csv.ReadHeader()) throw new HrValidationException("The CSV file does not contain a header row."); var headers = ValidateHeaders(csv.HeaderRecord ?? []); var rows = new List<ParsedCollectionRow>();
        while (await csv.ReadAsync()) { token.ThrowIfCancellationRequested(); if (rows.Count >= MaximumRows) throw new HrValidationException($"Collection imports cannot exceed {MaximumRows:N0} rows."); var values = headers.Select((header, index) => (header, value: Limit(csv.GetField(index)))).ToDictionary(x => x.header, x => x.value, StringComparer.OrdinalIgnoreCase); if (values.Values.All(string.IsNullOrWhiteSpace)) continue; rows.Add(new ParsedCollectionRow(csv.Parser.Row, values)); }
        if (rows.Count == 0) throw new HrValidationException("The import file does not contain data rows."); return rows;
    }

    private static IReadOnlyCollection<ParsedCollectionRow> ParseWorkbook(Stream stream, CancellationToken token)
    {
        var signature = new byte[4]; _ = stream.Read(signature); if (signature[0] != 0x50 || signature[1] != 0x4B) throw new HrValidationException("The XLSX file signature is invalid."); stream.Position = 0;
        using var reader = ExcelReaderFactory.CreateOpenXmlReader(stream, new ExcelReaderConfiguration { LeaveOpen = true }); if (!reader.Read()) throw new HrValidationException("The workbook is empty.");
        if (reader.FieldCount > MaximumColumns) throw new HrValidationException($"Collection imports cannot exceed {MaximumColumns} columns."); var rawHeaders = Enumerable.Range(0, reader.FieldCount).Select(i => Limit(reader.GetValue(i)?.ToString())).ToArray(); var headers = ValidateHeaders(rawHeaders); var rows = new List<ParsedCollectionRow>(); var rowNumber = 1;
        while (reader.Read()) { token.ThrowIfCancellationRequested(); rowNumber++; if (rows.Count >= MaximumRows) throw new HrValidationException($"Collection imports cannot exceed {MaximumRows:N0} rows."); var values = headers.Select((header, index) => (header, value: Limit(Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture)))).ToDictionary(x => x.header, x => x.value, StringComparer.OrdinalIgnoreCase); if (values.Values.All(string.IsNullOrWhiteSpace)) continue; rows.Add(new ParsedCollectionRow(rowNumber, values)); }
        if (rows.Count == 0) throw new HrValidationException("The workbook does not contain data rows in its first sheet."); return rows;
    }

    private static string[] ValidateHeaders(string[] headers)
    {
        if (headers.Length == 0 || headers.Length > MaximumColumns) throw new HrValidationException("The import header count is invalid."); var normalized = headers.Select(NormalizeHeader).ToArray(); if (normalized.Any(string.IsNullOrWhiteSpace)) throw new HrValidationException("Every import column must have a header."); if (normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Length) throw new HrValidationException("Import column headers must be unique."); return normalized;
    }
    internal static string NormalizeHeader(string? value) => new((value ?? string.Empty).Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
    private static string Limit(string? value) { var result = value?.Trim() ?? string.Empty; if (result.Length > 4000) throw new HrValidationException("An import cell exceeds 4,000 characters."); return result; }
}
