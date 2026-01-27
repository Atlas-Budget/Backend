using Api.Entities;

public interface ITokenService
{
    string CreateAccessToken(User user);
    Task<string> CreateRefreshTokenAsync(User user, bool rememberMe);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}
