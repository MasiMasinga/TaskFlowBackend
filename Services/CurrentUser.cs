using System.Security.Claims;
using TaskFlow.Interfaces;

namespace TaskFlow.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public bool IsAuthenticated =>
        _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var idClaim = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? _accessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }
}