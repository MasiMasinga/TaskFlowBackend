namespace TaskFlow.DTOs.Admin;

public record ClearUserCacheResponse(Guid UserId, int KeysRemoved);
