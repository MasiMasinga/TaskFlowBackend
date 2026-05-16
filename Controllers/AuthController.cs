using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.DTOs.Auth;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IClockService _clockService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IRefreshTokenService refreshTokenService,
        IClockService clockService)
    {
        _userManager = userManager;
        _refreshTokenService = refreshTokenService;
        _clockService = clockService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Email already registered",
                Detail = "An account with this email already exists."
            });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAtUtc = _clockService.GetClock().UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Registration failed",
                Detail = string.Join("; ", result.Errors.Select(e => e.Description))
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var tokens = await _refreshTokenService.IssueTokenPairAsync(user, roles, ct);

        return StatusCode(StatusCodes.Status201Created, ToAuthResponse(tokens));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials"
            });
        }

        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials"
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var tokens = await _refreshTokenService.IssueTokenPairAsync(user, roles, ct);
        return Ok(ToAuthResponse(tokens));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var tokens = await _refreshTokenService.RotateAsync(request.RefreshToken, ct);
        if (tokens is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid refresh token"
            });
        }

        return Ok(ToAuthResponse(tokens));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var revoked = await _refreshTokenService.RevokeAsync(request.RefreshToken, ct);
        if (!revoked)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Logout failed",
                Detail = "Refresh token is invalid or already revoked."
            });
        }

        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized"
            });
        }
        return Ok(new UserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc));
    }

    private static AuthResponse ToAuthResponse(AuthTokenPair tokens) =>
        new(
            tokens.AccessToken,
            tokens.AccessExpiresAtUtc,
            tokens.RefreshToken,
            tokens.RefreshExpiresAtUtc);
}
