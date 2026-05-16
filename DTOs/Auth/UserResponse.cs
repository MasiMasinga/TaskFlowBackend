namespace TaskFlow.DTOs.Auth;

public record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAtUtc
);