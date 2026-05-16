using TaskFlow.Models;

namespace TaskFlow.Interfaces;

public interface IRefreshTokenService
{
    Task<AuthTokenPair> IssueTokenPairAsync(ApplicationUser user, IList<string> roles, CancellationToken ct = default);
    Task<AuthTokenPair?> RotateAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> RevokeAsync(string refreshToken, CancellationToken ct = default);
}
