using System.Text.Json;

namespace MIS.Domain.Entities;

internal static class JsonText
{
    public static string? NormalizeOptional(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Normalize(value, parameterName);
    }

    public static string NormalizeRequired(string? value, string parameterName, string defaultValue = "[]") =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : Normalize(value, parameterName);

    private static string Normalize(string value, string parameterName)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.GetRawText();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Value must contain valid JSON.", parameterName, exception);
        }
    }
}
