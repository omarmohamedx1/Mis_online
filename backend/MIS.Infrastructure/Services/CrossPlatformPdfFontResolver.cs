using PdfSharp.Fonts;

namespace MIS.Infrastructure.Services;

internal sealed class CrossPlatformPdfFontResolver : IFontResolver
{
    internal const string FamilyName = "MIS Report Sans";
    private const string RegularFace = "MIS.ReportSans.Regular";
    private const string BoldFace = "MIS.ReportSans.Bold";
    private static readonly object RegistrationLock = new();
    private static readonly Lazy<FontFiles> Fonts = new(LoadFonts, LazyThreadSafetyMode.ExecutionAndPublication);

    internal static string EnsureRegistered()
    {
        lock (RegistrationLock)
        {
            if (GlobalFontSettings.FontResolver is null)
            {
                _ = Fonts.Value;
                GlobalFontSettings.FontResolver = new CrossPlatformPdfFontResolver();
            }
            else if (GlobalFontSettings.FontResolver is not CrossPlatformPdfFontResolver)
            {
                throw new InvalidOperationException(
                    "PDFsharp already has a different global font resolver. Configure the HR report font resolver before creating any PDF fonts.");
            }
        }

        return FamilyName;
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (!string.Equals(familyName, FamilyName, StringComparison.OrdinalIgnoreCase)) return null;

        var fonts = Fonts.Value;
        if (isBold && fonts.Bold is not null)
            return new FontResolverInfo(BoldFace, false, isItalic);

        return new FontResolverInfo(RegularFace, isBold, isItalic);
    }

    public byte[]? GetFont(string faceName) => faceName switch
    {
        RegularFace => Fonts.Value.Regular,
        BoldFace => Fonts.Value.Bold,
        _ => null
    };

    private static FontFiles LoadFonts()
    {
        var configuredRegular = Environment.GetEnvironmentVariable("MIS_PDF_FONT_PATH");
        var configuredBold = Environment.GetEnvironmentVariable("MIS_PDF_BOLD_FONT_PATH");
        if (!string.IsNullOrWhiteSpace(configuredRegular))
        {
            var regularPath = ValidateConfiguredPath(configuredRegular, "MIS_PDF_FONT_PATH");
            var boldPath = string.IsNullOrWhiteSpace(configuredBold)
                ? null
                : ValidateConfiguredPath(configuredBold, "MIS_PDF_BOLD_FONT_PATH");
            return Read(regularPath, boldPath);
        }

        foreach (var candidate in Candidates())
        {
            if (!File.Exists(candidate.Regular)) continue;
            var bold = candidate.Bold is not null && File.Exists(candidate.Bold) ? candidate.Bold : null;
            return Read(candidate.Regular, bold);
        }

        throw new InvalidOperationException(
            "No Unicode TrueType font was found for HR PDF reports. Install DejaVu Sans, Liberation Sans, Noto Sans, or Arial, " +
            "or set MIS_PDF_FONT_PATH and optionally MIS_PDF_BOLD_FONT_PATH to persistent .ttf files.");
    }

    private static string ValidateConfiguredPath(string value, string settingName)
    {
        var path = Path.GetFullPath(value.Trim());
        if (!string.Equals(Path.GetExtension(path), ".ttf", StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
            throw new InvalidOperationException($"{settingName} must point to an existing TrueType (.ttf) font file.");
        return path;
    }

    private static FontFiles Read(string regularPath, string? boldPath) => new(
        File.ReadAllBytes(regularPath),
        boldPath is null ? null : File.ReadAllBytes(boldPath));

    private static IEnumerable<FontCandidate> Candidates()
    {
        var applicationFonts = Path.Combine(AppContext.BaseDirectory, "Fonts");
        yield return Pair(applicationFonts, "NotoSans-Regular.ttf", "NotoSans-Bold.ttf");
        yield return Pair(applicationFonts, "DejaVuSans.ttf", "DejaVuSans-Bold.ttf");

        var windowsFonts = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        if (!string.IsNullOrWhiteSpace(windowsFonts))
            yield return Pair(windowsFonts, "arial.ttf", "arialbd.ttf");

        yield return Pair("/usr/share/fonts/truetype/dejavu", "DejaVuSans.ttf", "DejaVuSans-Bold.ttf");
        yield return Pair("/usr/share/fonts/truetype/liberation2", "LiberationSans-Regular.ttf", "LiberationSans-Bold.ttf");
        yield return Pair("/usr/share/fonts/truetype/liberation", "LiberationSans-Regular.ttf", "LiberationSans-Bold.ttf");
        yield return Pair("/usr/share/fonts/truetype/noto", "NotoSans-Regular.ttf", "NotoSans-Bold.ttf");
        yield return Pair("/usr/share/fonts/TTF", "DejaVuSans.ttf", "DejaVuSans-Bold.ttf");
        yield return Pair("/System/Library/Fonts/Supplemental", "Arial.ttf", "Arial Bold.ttf");
        yield return Pair("/Library/Fonts", "Arial.ttf", "Arial Bold.ttf");
    }

    private static FontCandidate Pair(string directory, string regular, string bold) =>
        new(Path.Combine(directory, regular), Path.Combine(directory, bold));

    private sealed record FontFiles(byte[] Regular, byte[]? Bold);
    private sealed record FontCandidate(string Regular, string? Bold);
}
