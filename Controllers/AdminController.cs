using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.DTOs.Admin;
using TaskFlow.DTOs.Auth;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminCacheService _adminCache;

    public AdminController(UserManager<ApplicationUser> userManager, IAdminCacheService adminCache)
    {
        _userManager = userManager;
        _adminCache = adminCache;
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

    [HttpPost("cache/clear")]
    [ProducesResponseType(typeof(ClearUserCacheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ClearUserCacheResponse>> ClearUserCache(
        [FromQuery] Guid userId,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "userId is required." });

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return NotFound(new { message = "User not found." });

        var keysRemoved = await _adminCache.ClearForUserAsync(userId, ct);
        return Ok(new ClearUserCacheResponse(userId, keysRemoved));
    }
}
