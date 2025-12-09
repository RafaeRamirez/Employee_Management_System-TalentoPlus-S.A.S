using System.Security.Claims;

namespace TalentoPlus.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(IEnumerable<Claim> claims, DateTime expires);
}
