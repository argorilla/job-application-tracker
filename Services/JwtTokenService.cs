using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobApplicationTracker.Models;
using Microsoft.IdentityModel.Tokens;

namespace JobApplicationTracker.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtTokenResult CreateToken(AppUser user)
    {
        var key = GetRequiredConfiguration("Jwt:Key");
        var issuer = GetRequiredConfiguration("Jwt:Issuer");
        var audience = GetRequiredConfiguration("Jwt:Audience");

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "JWT key must be at least 32 bytes.");
        }

        var expirationMinutes =
            _configuration.GetValue<int?>("Jwt:ExpirationMinutes")
            ?? 60;

        if (expirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT expiration must be greater than zero.");
        }

        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc =
            issuedAtUtc.AddMinutes(expirationMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.Username),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler()
            .WriteToken(jwt);

        return new JwtTokenResult(
            token,
            expiresAtUtc);
    }

    private string GetRequiredConfiguration(string key)
    {
        return _configuration[key]
            ?? throw new InvalidOperationException(
                $"Configuration '{key}' is missing.");
    }
}
