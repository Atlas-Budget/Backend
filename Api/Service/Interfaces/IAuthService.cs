using Api.Contracts;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshAsync(string refreshToken);
    Task LogoutAsync(string? refreshToken);
    Task<UserMeResponse> GetMeAsync(Guid userId);
}
