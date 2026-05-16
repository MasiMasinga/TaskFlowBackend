using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskFlow.Data;
using TaskFlow.Data.Configurations;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtConfiguration _jwt;
    private readonly IClockService _clockService;

    public RefreshTokenService(
        AppDbContext db,
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager,
        IOptions<JwtConfiguration> jwt,
        IClockService clockService)
    {
        _db = db;
        _tokenService = tokenService;
        _userManager = userManager;
        _jwt = jwt.Value;
        _clockService = clockService;
    }

    public async Task<AuthTokenPair> IssueTokenPairAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken ct = default)
    {
        var access = _tokenService.CreateAccessToken(user, roles);
        var refreshValue = GenerateToken();
        var refreshExpires = _clockService.GetClock().UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshValue,
            UserId = user.Id,
            ExpiresAtUtc = refreshExpires
        });

        await _db.SaveChangesAsync(ct);

        return new AuthTokenPair(
            access.AccessToken,
            access.ExpiresAtUtc,
            refreshValue,
            refreshExpires);
    }

    public async Task<AuthTokenPair?> RotateAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

        if (stored is null)
            return null;

        var now = _clockService.GetClock().UtcNow;

        if (stored.RevokedAtUtc is not null)
        {
            if (stored.ReplacedByToken is not null)
                await RevokeAllForUserAsync(stored.UserId, now, ct);

            return null;
        }

        if (stored.ExpiresAtUtc <= now)
            return null;

        var roles = await _userManager.GetRolesAsync(stored.User);
        var access = _tokenService.CreateAccessToken(stored.User, roles);
        var newRefreshValue = GenerateToken();
        var newRefreshExpires = now.AddDays(_jwt.RefreshTokenExpirationDays);

        stored.RevokedAtUtc = now;
        stored.ReplacedByToken = newRefreshValue;

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshValue,
            UserId = stored.UserId,
            ExpiresAtUtc = newRefreshExpires
        });

        await _db.SaveChangesAsync(ct);

        return new AuthTokenPair(
            access.AccessToken,
            access.ExpiresAtUtc,
            newRefreshValue,
            newRefreshExpires);
    }

    public async Task<bool> RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

        if (stored is null || stored.RevokedAtUtc is not null)
            return false;

        stored.RevokedAtUtc = _clockService.GetClock().UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task RevokeAllForUserAsync(Guid userId, DateTime revokedAtUtc, CancellationToken ct)
    {
        var active = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var token in active)
            token.RevokedAtUtc = revokedAtUtc;

        if (active.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    private static string GenerateToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
