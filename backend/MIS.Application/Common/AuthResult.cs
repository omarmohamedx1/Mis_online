using MIS.Application.DTOs.Auth;

namespace MIS.Application.Common;

public sealed class AuthResult
{
    private AuthResult(bool succeeded, AuthResponse? response, string? errorMessage)
    {
        Succeeded = succeeded;
        Response = response;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public AuthResponse? Response { get; }

    public string? ErrorMessage { get; }

    public static AuthResult Success(AuthResponse response)
    {
        return new AuthResult(true, response, null);
    }

    public static AuthResult Failure(string errorMessage)
    {
        return new AuthResult(false, null, errorMessage);
    }
}
