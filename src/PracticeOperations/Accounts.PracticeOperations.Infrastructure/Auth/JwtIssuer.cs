using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;
using Accounts.SharedKernel.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Accounts.PracticeOperations.Infrastructure.Auth;

public sealed class JwtIssuer : IJwtIssuer
{
    private readonly IClock _clock;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SigningCredentials _credentials;
    private readonly TimeSpan _lifetime;

    public JwtIssuer(IConfiguration config, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(config);
        _clock = clock;
        _issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        _audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret missing");
        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);
        _lifetime = TimeSpan.FromMinutes(
            int.Parse(config["Jwt:LifetimeMinutes"] ?? "60", CultureInfo.InvariantCulture));
    }

    public (string Token, DateTimeOffset ExpiresAt) Issue(FirmId firmId, UserId userId, Role role)
    {
        var expires = _clock.UtcNow.Add(_lifetime);
        var claims = new[]
        {
            new Claim("sub", userId.Value.ToString()),
            new Claim("firm_id", firmId.Value.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
        };
        var token = new JwtSecurityToken(
            issuer: _issuer, audience: _audience,
            claims: claims,
            notBefore: _clock.UtcNow.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: _credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
