namespace TaskFlow.DTOs.Auth;

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc);