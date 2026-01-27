using Api.Contracts;
using Api.Data;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task RegisterAsync(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("Email já cadastrado.");

        var user = new User
        {
            Name = req.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task<AuthResult> LoginAsync(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email)
            ?? throw new UnauthorizedAccessException();

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException();

        return await CreateSessionAsync(user, req.RememberMe);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken)
    {
        var rt = await _tokenService.ValidateRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException();

        return await CreateSessionAsync(rt.User, rememberMe: true);
    }

    public async Task LogoutAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task<UserMeResponse> GetMeAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new UnauthorizedAccessException();

        return new UserMeResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc);
    }

    private async Task<AuthResult> CreateSessionAsync(User user, bool rememberMe)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user, rememberMe);

        return new AuthResult(
            accessToken,
            refreshToken,
            rememberMe,
            new UserMeResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc)
        );
    }
}
