using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var secret = _configuration["Jwt__Secret"] ?? throw new InvalidOperationException("JWT secret missing");
        var issuer = _configuration["Jwt__Issuer"] ?? "TalentoPlus.Api";
        var audience = _configuration["Jwt__Audience"] ?? "TalentoPlus.Client";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        // Minimal JWT with symmetric signing; expiry controlled by caller.
        var tokenDescriptor = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
