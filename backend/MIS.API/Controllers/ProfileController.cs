using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.Application.DTOs.Auth;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IUserProfileService _service;
    public ProfileController(IUserProfileService service) => _service = service;

    [HttpGet]
    public Task<UserProfileDto> Get(CancellationToken token) => _service.GetAsync(token);

    [HttpPut("email")]
    public Task<UserProfileDto> ChangeEmail(ChangeMyEmailRequest request, CancellationToken token) => _service.ChangeEmailAsync(request, token);

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangeMyPasswordRequest request, CancellationToken token)
    {
        await _service.ChangePasswordAsync(request, token);
        return NoContent();
    }
}
