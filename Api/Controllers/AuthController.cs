using Api.Contracts;
using Api.Data;
using Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public AuthController(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
    {
        _db = db;
        _config = config;
        _env = env;
    }

    // ---------------------------
    // POST /api/auth/register
    // ---------------------------
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest req)
    {
        var name = req.Name.Trim();
        var email = req.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email é obrigatório.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6) return BadRequest("Senha muito curta.");

        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists) return BadRequest("Email já cadastrado.");

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok();
    }

    // ---------------------------
    // POST /api/auth/login
    // ---------------------------
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return Unauthorized("Credenciais inválidas.");

        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok) return Unauthorized("Credenciais inválidas.");

        var accessToken = CreateAccessToken(user);

        // refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshHash = Sha256(refreshToken);

        var expires = req.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(1);

        var rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expires
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        Response.Cookies.Append(
            "refresh_token",
            refreshToken,
            BuildRefreshCookieOptions(req.RememberMe)
        );

        return new AuthResponse(
            accessToken,
            new UserMeResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc)
        );
    }

    // ---------------------------
    // POST /api/auth/refresh
    // ---------------------------
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized("Sem refresh token.");

        var hash = Sha256(refreshToken);

        var rt = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (rt is null || rt.RevokedAtUtc != null || rt.ExpiresAtUtc <= DateTime.UtcNow)
            return Unauthorized("Refresh inválido.");

        // rotacionar refresh token
        rt.RevokedAtUtc = DateTime.UtcNow;

        var newRefresh = GenerateRefreshToken();
        var newHash = Sha256(newRefresh);

        var newRt = new RefreshToken
        {
            UserId = rt.UserId,
            TokenHash = newHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30) // padrão; depois você pode guardar rememberMe no user
        };

        _db.RefreshTokens.Add(newRt);

        var accessToken = CreateAccessToken(rt.User);
        await _db.SaveChangesAsync();

        Response.Cookies.Append("refresh_token", newRefresh, BuildRefreshCookieOptions(rememberMe: true));

        return new AuthResponse(
            accessToken,
            new UserMeResponse(rt.User.Id, rt.User.Name, rt.User.Email, rt.User.CreatedAtUtc)
        );
    }

    // ---------------------------
    // POST /api/auth/logout
    // ---------------------------
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refresh_token", out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            var hash = Sha256(refreshToken);
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);
            if (rt is not null)
            {
                rt.RevokedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refresh_token");
        return Ok();
    }

    // ---------------------------
    // GET /api/auth/me (protegido)
    // ---------------------------
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserMeResponse>> Me()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (sub is null) return Unauthorized();

        var userId = Guid.Parse(sub);
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        return new UserMeResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc);
    }

    // ===========================
    // Helpers
    // ===========================
    private string CreateAccessToken(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name)
        };

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
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

    private CookieOptions BuildRefreshCookieOptions(bool rememberMe)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,                // obrigatório com SameSite=None
            SameSite = SameSiteMode.None, // essencial para cross-origin
            Expires = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddDays(1),
            Path = "/"
        };
    }
}
