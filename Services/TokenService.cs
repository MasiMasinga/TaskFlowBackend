using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Data.Configurations;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class TokenService : ITokenService
{
    private readonly JwtConfiguration _options;
    private readonly IClockService _clockService;

    public TokenService(IOptions<JwtConfiguration> options, IClockService clockService)
    {
        _options = options.Value;
        _clockService = clockService;
    }

    public TokenResult CreateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var now = _clockService.GetClock().UtcNow;
        var expires = now.AddMinutes(_options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("displayName", user.DisplayName)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenResult(jwt, expires);
    }
}