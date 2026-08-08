namespace MIS.Application.Common;

public sealed record ApiErrorResponse(bool Success, string Message, IReadOnlyCollection<string> Errors)
{
    public static ApiErrorResponse Failure(string message, IEnumerable<string>? errors = null)
    {
        return new ApiErrorResponse(false, message, errors?.ToArray() ?? []);
    }
}
