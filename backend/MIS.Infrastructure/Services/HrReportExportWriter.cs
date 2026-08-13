using ClosedXML.Excel;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Globalization;
using System.Text;

namespace MIS.Infrastructure.Services;

internal static class HrReportExportWriter
{
    public static byte[] WriteExcel(
        string reportName,
        IReadOnlyCollection<HrReportColumnDto> columns,
        IReadOnlyCollection<HrReportRowDto> rows,
        IReadOnlyDictionary<string, string> filters,
        DateTimeOffset generatedAt)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(SanitizeWorksheetName(reportName));
        worksheet.RightToLeft = ApiTextLocalizer.IsArabic;
        var columnCount = Math.Max(1, columns.Count);
        var layoutColumnCount = Math.Max(2, columnCount);
        using (var logoStream = OpenLogoStream())
        {
            worksheet.AddPicture(logoStream, "MIS Collection Firm Logo")
                .MoveTo(worksheet.Cell(1, 1))
                .WithSize(72, 69);
        }
        worksheet.Row(1).Height = 54;
        worksheet.Cell(1, 2).Value = reportName;
        worksheet.Range(1, 2, 1, layoutColumnCount).Merge();
        worksheet.Cell(1, 2).Style.Font.Bold = true;
        worksheet.Cell(1, 2).Style.Font.FontSize = 16;
        worksheet.Cell(1, 2).Style.Font.FontColor = XLColor.White;
        worksheet.Cell(1, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#10255C");
        worksheet.Cell(1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Cell(2, 2).Value = ApiTextLocalizer.IsArabic
            ? $"تاريخ الإنشاء: {generatedAt:yyyy-MM-dd HH:mm 'UTC'}"
            : $"Generated: {generatedAt:yyyy-MM-dd HH:mm 'UTC'}";
        worksheet.Range(2, 2, 2, layoutColumnCount).Merge();
        var rowNumber = 4;
        if (filters.Count > 0)
        {
            worksheet.Cell(rowNumber, 1).Value = ApiTextLocalizer.Localize("Applied Filters");
            worksheet.Cell(rowNumber, 1).Style.Font.Bold = true;
            rowNumber++;
            foreach (var filter in filters)
            {
                worksheet.Cell(rowNumber, 1).Value = filter.Key;
                worksheet.Cell(rowNumber, 2).Value = filter.Value;
                rowNumber++;
            }
        }

        rowNumber++;
        var headerRow = rowNumber;
        var columnIndex = 1;
        foreach (var column in columns)
        {
            worksheet.Cell(headerRow, columnIndex).Value = column.Header;
            columnIndex++;
        }
        var headerRange = worksheet.Range(headerRow, 1, headerRow, columnCount);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B638F");
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        foreach (var row in rows)
        {
            rowNumber++;
            columnIndex = 1;
            foreach (var column in columns)
            {
                worksheet.Cell(rowNumber, columnIndex).Value = row.Values.GetValueOrDefault(column.Key) ?? string.Empty;
                columnIndex++;
            }
        }

        if (rows.Count > 0)
        {
            var tableRange = worksheet.Range(headerRow, 1, rowNumber, columnCount);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            tableRange.SetAutoFilter();
        }
        worksheet.SheetView.FreezeRows(headerRow);
        worksheet.Columns(1, columnCount).AdjustToContents();
        foreach (var column in worksheet.Columns(1, columnCount))
        {
            if (column.Width > 45) column.Width = 45;
            if (column.Width < 10) column.Width = 10;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] WritePdf(
        string reportName,
        IReadOnlyCollection<HrReportColumnDto> columns,
        IReadOnlyCollection<HrReportRowDto> rows,
        IReadOnlyDictionary<string, string> filters,
        DateTimeOffset generatedAt)
    {
        using var document = new PdfDocument();
        document.Info.Title = reportName;
        var rightToLeft = ApiTextLocalizer.IsArabic;
        document.Language = rightToLeft ? "ar" : "en";
        document.ViewerPreferences.Direction = rightToLeft
            ? PdfReadingDirection.RightToLeft
            : PdfReadingDirection.LeftToRight;
        var fontFamily = CrossPlatformPdfFontResolver.EnsureRegistered();
        var fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.EmbedCompleteFontFile);
        var titleFont = new XFont(fontFamily, 16, XFontStyleEx.Bold, fontOptions);
        var metaFont = new XFont(fontFamily, 8, XFontStyleEx.Regular, fontOptions);
        var headerFont = new XFont(fontFamily, 7, XFontStyleEx.Bold, fontOptions);
        var cellFont = new XFont(fontFamily, 7, XFontStyleEx.Regular, fontOptions);
        using var logoStream = OpenLogoStream();
        using var logo = XImage.FromStream(logoStream);
        var sourceColumns = columns.Count == 0
            ? [new HrReportColumnDto("value", ApiTextLocalizer.Localize("Value"))]
            : columns.ToArray();
        var columnGroups = BuildPdfColumnGroups(sourceColumns);
        const double margin = 28d;
        const double rowHeight = 22d;

        for (var groupIndex = 0; groupIndex < columnGroups.Count; groupIndex++)
        {
            var logicalColumns = columnGroups[groupIndex];
            var columnList = rightToLeft ? logicalColumns.Reverse().ToArray() : logicalColumns;
            PdfPage? page = null;
            XGraphics? graphics = null;
            var y = 0d;

            void NewPage(bool includeMetadata)
            {
                graphics?.Dispose();
                page = document.AddPage();
                page.Size = PageSize.A4;
                page.Orientation = PageOrientation.Landscape;
                graphics = XGraphics.FromPdfPage(page);
                y = margin;
                const double logoHeight = 48d;
                const double logoWidth = 50d;
                var logoX = rightToLeft ? page.Width.Point - margin - logoWidth : margin;
                graphics.DrawImage(logo, logoX, y, logoWidth, logoHeight);
                var titleX = rightToLeft ? margin : margin + logoWidth + 10;
                var titleWidth = page.Width.Point - (margin * 2) - logoWidth - 10;
                graphics.DrawString(
                    rightToLeft ? PreparePdfText(reportName, true) : reportName,
                    titleFont,
                    XBrushes.DarkBlue,
                    new XRect(titleX, y + 12, titleWidth, 24),
                    rightToLeft ? XStringFormats.TopRight : XStringFormats.TopLeft);
                y += logoHeight + 5;

                if (columnGroups.Count > 1)
                {
                    var sectionLabel = rightToLeft
                        ? $"مجموعة الحقول {groupIndex + 1} من {columnGroups.Count}"
                        : $"Column group {groupIndex + 1} of {columnGroups.Count}";
                    DrawMetadata(sectionLabel, XBrushes.DarkBlue);
                    y += 12;
                }

                if (includeMetadata)
                {
                    var generatedLabel = rightToLeft
                        ? $"تاريخ الإنشاء: {generatedAt:yyyy-MM-dd HH:mm 'UTC'}"
                        : $"Generated: {generatedAt:yyyy-MM-dd HH:mm 'UTC'}";
                    DrawMetadata(generatedLabel, XBrushes.Black);
                    y += 14;
                    foreach (var filter in filters)
                    {
                        DrawMetadata($"{filter.Key}: {filter.Value}", XBrushes.Black);
                        y += 12;
                    }
                }
                y += 6;
                DrawHeader();

                void DrawMetadata(string value, XBrush brush)
                {
                    if (page is null || graphics is null) return;
                    graphics.DrawString(
                        rightToLeft ? PreparePdfText(value, true) : value,
                        metaFont,
                        brush,
                        new XRect(margin, y, page.Width.Point - (margin * 2), 12),
                        rightToLeft ? XStringFormats.TopRight : XStringFormats.TopLeft);
                }
            }

            void DrawHeader()
            {
                if (page is null || graphics is null) return;
                var usableWidth = page.Width.Point - (margin * 2);
                var columnWidth = usableWidth / columnList.Length;
                for (var index = 0; index < columnList.Length; index++)
                {
                    var x = margin + (index * columnWidth);
                    graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(11, 99, 143)), x, y, columnWidth, rowHeight);
                    graphics.DrawString(
                        FitText(graphics, columnList[index].Header, headerFont, columnWidth - 8, rightToLeft),
                        headerFont, XBrushes.White,
                        new XRect(x + 4, y + 5, columnWidth - 8, rowHeight - 4),
                        rightToLeft ? XStringFormats.TopRight : XStringFormats.TopLeft);
                }
                y += rowHeight;
            }

            NewPage(groupIndex == 0);
            foreach (var row in rows)
            {
                if (page is null || graphics is null) break;
                if (y + rowHeight > page.Height.Point - margin) NewPage(false);
                var usableWidth = page!.Width.Point - (margin * 2);
                var columnWidth = usableWidth / columnList.Length;
                for (var index = 0; index < columnList.Length; index++)
                {
                    var x = margin + (index * columnWidth);
                    graphics!.DrawRectangle(XPens.LightGray, x, y, columnWidth, rowHeight);
                    var value = row.Values.GetValueOrDefault(columnList[index].Key) ?? string.Empty;
                    graphics.DrawString(
                        FitText(graphics, value, cellFont, columnWidth - 8, rightToLeft),
                        cellFont, XBrushes.Black,
                        new XRect(x + 4, y + 5, columnWidth - 8, rowHeight - 4),
                        rightToLeft ? XStringFormats.TopRight : XStringFormats.TopLeft);
                }
                y += rowHeight;
            }

            if (rows.Count == 0 && graphics is not null && page is not null)
            {
                var noData = rightToLeft ? "لا توجد بيانات مطابقة للفلاتر المطبقة." : "No data matched the applied filters.";
                graphics.DrawString(
                    rightToLeft ? PreparePdfText(noData, true) : noData,
                    cellFont, XBrushes.Black,
                    new XRect(margin, y + 4, page.Width.Point - (margin * 2), rowHeight),
                    rightToLeft ? XStringFormats.TopRight : XStringFormats.TopLeft);
            }
            graphics?.Dispose();
        }

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static IReadOnlyList<HrReportColumnDto[]> BuildPdfColumnGroups(HrReportColumnDto[] columns)
    {
        const int maximumColumnsPerPage = 6;
        const int repeatedIdentityColumns = 2;
        if (columns.Length <= maximumColumnsPerPage) return [columns];

        var identity = columns.Take(repeatedIdentityColumns).ToArray();
        var result = new List<HrReportColumnDto[]>();
        for (var index = repeatedIdentityColumns; index < columns.Length; index += maximumColumnsPerPage - repeatedIdentityColumns)
        {
            result.Add(identity.Concat(columns.Skip(index).Take(maximumColumnsPerPage - repeatedIdentityColumns)).ToArray());
        }
        return result;
    }

    private static string FitText(XGraphics graphics, string value, XFont font, double maximumWidth, bool rightToLeft)
    {
        var renderedValue = PreparePdfText(value, rightToLeft);
        if (graphics.MeasureString(renderedValue, font).Width <= maximumWidth) return renderedValue;
        const string suffix = "…";
        var textElementStarts = StringInfo.ParseCombiningCharacters(value);
        var elementCount = textElementStarts.Length;
        while (elementCount > 0)
        {
            var end = elementCount < textElementStarts.Length ? textElementStarts[elementCount] : value.Length;
            var candidate = PreparePdfText(value[..end] + suffix, rightToLeft);
            if (graphics.MeasureString(candidate, font).Width <= maximumWidth) return candidate;
            elementCount--;
        }
        return string.Empty;
    }

    private static Stream OpenLogoStream() =>
        typeof(HrReportExportWriter).Assembly.GetManifestResourceStream("MIS.Infrastructure.Assets.mis-logo.png")
        ?? throw new InvalidOperationException("The MIS report logo resource is missing.");

    /// <summary>
    /// PDFsharp maps Unicode code points to glyphs but does not apply OpenType Arabic shaping or
    /// the Unicode bidirectional algorithm. Convert Arabic letters to their contextual presentation
    /// forms, then arrange directional runs in visual order before handing text to DrawString.
    /// Latin text is returned byte-for-byte unchanged.
    /// </summary>
    private static string PreparePdfText(string value, bool rightToLeft)
    {
        if (!rightToLeft || string.IsNullOrEmpty(value)) return value;
        return ReorderRightToLeftRuns(ShapeArabic(value));
    }

    private static string ShapeArabic(string value)
    {
        var result = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (!TryGetArabicForms(character, out var forms))
            {
                result.Append(character);
                continue;
            }

            if (character == '\u0644' && index + 1 < value.Length &&
                TryGetLamAlefForms(value[index + 1], out var isolatedLigature, out var finalLigature))
            {
                var connectsToPrevious = TryFindPreviousArabicForms(value, index, out var previousForms) &&
                                         previousForms.ConnectsToNext && forms.ConnectsToPrevious;
                result.Append(connectsToPrevious ? finalLigature : isolatedLigature);
                index++;
                continue;
            }

            var connectsPrevious = TryFindPreviousArabicForms(value, index, out var previous) &&
                                   previous.ConnectsToNext && forms.ConnectsToPrevious;
            var connectsNext = TryFindNextArabicForms(value, index, out var next) &&
                               forms.ConnectsToNext && next.ConnectsToPrevious;
            result.Append(forms.Select(connectsPrevious, connectsNext));
        }
        return result.ToString();
    }

    private static string ReorderRightToLeftRuns(string value)
    {
        var starts = StringInfo.ParseCombiningCharacters(value);
        if (starts.Length == 0) return value;

        var elements = new string[starts.Length];
        var directions = new TextDirection[starts.Length];
        for (var index = 0; index < starts.Length; index++)
        {
            var end = index + 1 < starts.Length ? starts[index + 1] : value.Length;
            elements[index] = value[starts[index]..end];
            directions[index] = GetDirection(elements[index]);
        }

        // Resolve neutral punctuation and spacing using the surrounding strong runs. If the
        // surrounding directions differ, Unicode bidi resolves them to the paragraph direction.
        for (var index = 0; index < directions.Length; index++)
        {
            if (directions[index] != TextDirection.Neutral) continue;
            var start = index;
            while (index + 1 < directions.Length && directions[index + 1] == TextDirection.Neutral) index++;
            var before = start > 0 ? directions[start - 1] : TextDirection.RightToLeft;
            var after = index + 1 < directions.Length ? directions[index + 1] : TextDirection.RightToLeft;
            var resolved = before == after ? before : TextDirection.RightToLeft;
            for (var neutralIndex = start; neutralIndex <= index; neutralIndex++) directions[neutralIndex] = resolved;
        }

        var runs = new List<TextRun>();
        var runStart = 0;
        for (var index = 1; index <= elements.Length; index++)
        {
            if (index < elements.Length && directions[index] == directions[runStart]) continue;
            runs.Add(new TextRun(runStart, index - runStart, directions[runStart]));
            runStart = index;
        }

        var result = new StringBuilder(value.Length);
        for (var runIndex = runs.Count - 1; runIndex >= 0; runIndex--)
        {
            var run = runs[runIndex];
            if (run.Direction == TextDirection.LeftToRight)
            {
                for (var index = run.Start; index < run.Start + run.Length; index++) result.Append(elements[index]);
            }
            else
            {
                for (var index = run.Start + run.Length - 1; index >= run.Start; index--) result.Append(elements[index]);
            }
        }
        return result.ToString();
    }

    private static TextDirection GetDirection(string textElement)
    {
        foreach (var rune in textElement.EnumerateRunes())
        {
            var category = Rune.GetUnicodeCategory(rune);
            // Both Western and Arabic-Indic numbers keep their left-to-right digit order.
            if (category is UnicodeCategory.DecimalDigitNumber or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber)
                return TextDirection.LeftToRight;
            if (category is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or
                UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter)
                return IsArabicCodePoint(rune.Value) ? TextDirection.RightToLeft : TextDirection.LeftToRight;
        }
        return TextDirection.Neutral;
    }

    private static bool IsArabicCodePoint(int codePoint) =>
        codePoint is >= 0x0600 and <= 0x06FF or >= 0x0750 and <= 0x077F or
            >= 0x08A0 and <= 0x08FF or >= 0xFB50 and <= 0xFDFF or >= 0xFE70 and <= 0xFEFF;

    private static bool TryFindPreviousArabicForms(string value, int index, out ArabicForms forms)
    {
        for (var candidate = index - 1; candidate >= 0; candidate--)
        {
            if (IsTransparentArabicMark(value[candidate])) continue;
            return TryGetArabicForms(value[candidate], out forms);
        }
        forms = default;
        return false;
    }

    private static bool TryFindNextArabicForms(string value, int index, out ArabicForms forms)
    {
        for (var candidate = index + 1; candidate < value.Length; candidate++)
        {
            if (IsTransparentArabicMark(value[candidate])) continue;
            return TryGetArabicForms(value[candidate], out forms);
        }
        forms = default;
        return false;
    }

    private static bool IsTransparentArabicMark(char value) =>
        CharUnicodeInfo.GetUnicodeCategory(value) is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark;

    private static bool TryGetLamAlefForms(char value, out char isolated, out char final)
    {
        (isolated, final) = value switch
        {
            '\u0622' => ('\uFEF5', '\uFEF6'),
            '\u0623' => ('\uFEF7', '\uFEF8'),
            '\u0625' => ('\uFEF9', '\uFEFA'),
            '\u0627' => ('\uFEFB', '\uFEFC'),
            _ => default
        };
        return isolated != '\0';
    }

    private static bool TryGetArabicForms(char value, out ArabicForms forms)
    {
        forms = value switch
        {
            '\u0621' => new('\uFE80'),
            '\u0622' => new('\uFE81', '\uFE82'),
            '\u0623' => new('\uFE83', '\uFE84'),
            '\u0624' => new('\uFE85', '\uFE86'),
            '\u0625' => new('\uFE87', '\uFE88'),
            '\u0626' => new('\uFE89', '\uFE8A', '\uFE8B', '\uFE8C'),
            '\u0627' => new('\uFE8D', '\uFE8E'),
            '\u0628' => new('\uFE8F', '\uFE90', '\uFE91', '\uFE92'),
            '\u0629' => new('\uFE93', '\uFE94'),
            '\u062A' => new('\uFE95', '\uFE96', '\uFE97', '\uFE98'),
            '\u062B' => new('\uFE99', '\uFE9A', '\uFE9B', '\uFE9C'),
            '\u062C' => new('\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0'),
            '\u062D' => new('\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4'),
            '\u062E' => new('\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8'),
            '\u062F' => new('\uFEA9', '\uFEAA'),
            '\u0630' => new('\uFEAB', '\uFEAC'),
            '\u0631' => new('\uFEAD', '\uFEAE'),
            '\u0632' => new('\uFEAF', '\uFEB0'),
            '\u0633' => new('\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4'),
            '\u0634' => new('\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8'),
            '\u0635' => new('\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC'),
            '\u0636' => new('\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0'),
            '\u0637' => new('\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4'),
            '\u0638' => new('\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8'),
            '\u0639' => new('\uFEC9', '\uFECA', '\uFECB', '\uFECC'),
            '\u063A' => new('\uFECD', '\uFECE', '\uFECF', '\uFED0'),
            '\u0640' => new('\u0640', '\u0640', '\u0640', '\u0640'),
            '\u0641' => new('\uFED1', '\uFED2', '\uFED3', '\uFED4'),
            '\u0642' => new('\uFED5', '\uFED6', '\uFED7', '\uFED8'),
            '\u0643' => new('\uFED9', '\uFEDA', '\uFEDB', '\uFEDC'),
            '\u0644' => new('\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0'),
            '\u0645' => new('\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4'),
            '\u0646' => new('\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8'),
            '\u0647' => new('\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC'),
            '\u0648' => new('\uFEED', '\uFEEE'),
            '\u0649' => new('\uFEEF', '\uFEF0'),
            '\u064A' => new('\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4'),
            '\u0671' => new('\uFB50', '\uFB51'),
            '\u0679' => new('\uFB66', '\uFB67', '\uFB68', '\uFB69'),
            '\u067E' => new('\uFB56', '\uFB57', '\uFB58', '\uFB59'),
            '\u0686' => new('\uFB7A', '\uFB7B', '\uFB7C', '\uFB7D'),
            '\u0688' => new('\uFB88', '\uFB89'),
            '\u0691' => new('\uFB8C', '\uFB8D'),
            '\u0698' => new('\uFB8A', '\uFB8B'),
            '\u06A4' => new('\uFB6A', '\uFB6B', '\uFB6C', '\uFB6D'),
            '\u06A9' => new('\uFB8E', '\uFB8F', '\uFB90', '\uFB91'),
            '\u06AF' => new('\uFB92', '\uFB93', '\uFB94', '\uFB95'),
            '\u06BA' => new('\uFB9E', '\uFB9F'),
            '\u06BE' => new('\uFBAA', '\uFBAB', '\uFBAC', '\uFBAD'),
            '\u06C0' => new('\uFBA4', '\uFBA5'),
            '\u06C1' => new('\uFBA6', '\uFBA7', '\uFBA8', '\uFBA9'),
            '\u06CC' => new('\uFBFC', '\uFBFD', '\uFBFE', '\uFBFF'),
            _ => default
        };
        return forms.IsDefined;
    }

    private readonly record struct ArabicForms(char Isolated, char Final = '\0', char Initial = '\0', char Medial = '\0')
    {
        public bool IsDefined => Isolated != '\0';
        public bool ConnectsToPrevious => Final != '\0';
        public bool ConnectsToNext => Initial != '\0';

        public char Select(bool connectsPrevious, bool connectsNext)
        {
            if (connectsPrevious && connectsNext && Medial != '\0') return Medial;
            if (connectsPrevious && Final != '\0') return Final;
            if (connectsNext && Initial != '\0') return Initial;
            return Isolated;
        }
    }

    private enum TextDirection { Neutral, LeftToRight, RightToLeft }
    private readonly record struct TextRun(int Start, int Length, TextDirection Direction);

    private static string SanitizeWorksheetName(string value)
    {
        var sanitized = string.Concat(value.Select(character => "[]:*?/\\".Contains(character) ? '-' : character));
        return sanitized.Length <= 31 ? sanitized : sanitized[..31];
    }
}
