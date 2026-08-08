namespace MIS.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumSecretBytes = 32;

    public string Issuer { get; init; } = "MIS";

    public string Audience { get; init; } = "MIS.Frontend";

    public string SecretKey { get; init; } = string.Empty;

    public int ExpiresInMinutes { get; init; } = 60;
}
