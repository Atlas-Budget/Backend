using Api.Data;
using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Service.Services;

public class TokenService : ITokenService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public TokenService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // ===========================
    // ACCESS TOKEN (JWT)
    // ===========================
    public string CreateAccessToken(User user)
    {
        var jwt = _config.GetSection("Jwt");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name)
        };

        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(jwt["AccessTokenMinutes"]!)
        );

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ===========================
    // REFRESH TOKEN
    // ===========================
    public async Task<string> CreateRefreshTokenAsync(User user, bool rememberMe)
    {
        var rawToken = GenerateSecureToken();
        var hash = Sha256(rawToken);

        var expires = rememberMe
            ? DateTime.UtcNow.AddDays(30)
            : DateTime.UtcNow.AddDays(1);

        var rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expires
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        return rawToken;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var hash = Sha256(refreshToken);

        var rt = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (rt is null) return null;
        if (rt.RevokedAtUtc != null) return null;
        if (rt.ExpiresAtUtc <= DateTime.UtcNow) return null;

        // rotação: revoga o atual
        rt.RevokedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return rt;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var hash = Sha256(refreshToken);

        var rt = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (rt is null) return;

        rt.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ===========================
    // HELPERS
    // ===========================
    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
