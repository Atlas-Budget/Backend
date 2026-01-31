namespace Api.Contracts;

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password, bool RememberMe);

public record UserMeResponse(Guid Id, string Name, string Email, DateTime CreatedAtUtc);
public record AuthResponse(string AccessToken, UserMeResponse User);

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    bool RememberMe,
    UserMeResponse User
);
