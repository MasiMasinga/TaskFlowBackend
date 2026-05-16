using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.DTOs.Auth;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IClockService _clockService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService, IClockService clockService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
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
        var token = _tokenService.CreateAccessToken(user, roles);

        return StatusCode(
            StatusCodes.Status201Created,
            new AuthResponse(token.AccessToken, token.ExpiresAtUtc));
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
        var token = _tokenService.CreateAccessToken(user, roles);
        return Ok(new AuthResponse(token.AccessToken, token.ExpiresAtUtc));
    }
}