using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}