using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amnecyne.LinkShort.DTOs;
using Amnecyne.LinkShort.Models;
using Microsoft.IdentityModel.Tokens;

namespace Amnecyne.LinkShort.Services;

public class TokenService(IConfiguration config)
{
    public TokenResponse GenerateTokens(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            config["Jwt:Issuer"],
            config["Jwt:Audience"],
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username)
            },
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        var refreshToken = new RefreshTokens
        {
            UserId = user.Id.ToString(),
            Token = Guid.NewGuid().ToString(),
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            RefreshToken = refreshToken.Token
        };
    }
}