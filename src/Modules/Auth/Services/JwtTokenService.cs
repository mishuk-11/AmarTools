using AmarTools.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmarTools.Modules.Auth.Services;

/// <summary>
/// Generates HS256-signed JWT access tokens that include role claims
/// so <c>[Authorize(Roles = "...")]</c> works out of the box.
/// </summary>
internal sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime ExpiresAt) CreateToken(
        ApplicationUser user, IEnumerable<string> roles)
    {
        var key      = GetRequiredConfig("Jwt:Key");
        var issuer   = GetRequiredConfig("Jwt:Issuer");
        var audience = GetRequiredConfig("Jwt:Audience");
        var expiry   = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

        var signingKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name,  user.FullName),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier,     user.Id.ToString()),
            new(ClaimTypes.Email,              user.Email),
            new(ClaimTypes.Name,               user.FullName),
        };

        // One ClaimTypes.Role claim per role — ASP.NET Core reads all of them
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var expiresAt = DateTime.UtcNow.AddMinutes(expiry);

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private string GetRequiredConfig(string key) =>
        _config[key] ?? throw new InvalidOperationException(
            $"Missing required JWT configuration: '{key}'. " +
            "Add it to appsettings.json under the 'Jwt' section.");
}
