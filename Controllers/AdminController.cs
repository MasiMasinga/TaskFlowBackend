using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.DTOs.Auth;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsers(CancellationToken ct)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .Select(u => new UserResponse(
                u.Id,
                u.Email ?? string.Empty,
                u.DisplayName,
                u.CreatedAtUtc))
            .ToListAsync(ct);

        return Ok(users);
    }
}
