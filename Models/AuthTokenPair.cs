namespace TaskFlow.Models;

public record AuthTokenPair(
    string AccessToken,
    DateTime AccessExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshExpiresAtUtc);
