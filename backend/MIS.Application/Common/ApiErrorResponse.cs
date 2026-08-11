namespace MIS.Application.Common;

public sealed record ApiErrorResponse(bool Success, string Message, IReadOnlyCollection<string> Errors)
{
    public static ApiErrorResponse Failure(string message, IEnumerable<string>? errors = null)
    {
        return new ApiErrorResponse(
            false,
            ApiTextLocalizer.Localize(message, useGenericArabicFallback: true),
            ApiTextLocalizer.LocalizeErrors(errors));
    }
}
