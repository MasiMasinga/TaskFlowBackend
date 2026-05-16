using TaskFlow.Models;

namespace TaskFlow.Interfaces;

public interface ITokenService
{
    TokenResult CreateAccessToken(ApplicationUser user, IList<string> roles);
}