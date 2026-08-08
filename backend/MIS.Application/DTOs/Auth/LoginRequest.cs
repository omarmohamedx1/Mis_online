using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Auth;

public sealed record LoginRequest
{
    [Required]
    [StringLength(128)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string Password { get; init; } = string.Empty;
}
