using Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        await _auth.RegisterAsync(req);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var result = await _auth.LoginAsync(req);

        Response.Cookies.Append(
            "refresh_token",
            result.RefreshToken,
            BuildCookie(result.RememberMe)
        );

        return new AuthResponse(result.AccessToken, result.User);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var token))
            return Unauthorized();

        var result = await _auth.RefreshAsync(token);

        Response.Cookies.Append(
            "refresh_token",
            result.RefreshToken,
            BuildCookie(true)
        );

        return new AuthResponse(result.AccessToken, result.User);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserMeResponse>> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        return await _auth.GetMeAsync(userId);
    }

    private static CookieOptions BuildCookie(bool rememberMe) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = rememberMe
            ? DateTimeOffset.UtcNow.AddDays(30)
            : DateTimeOffset.UtcNow.AddDays(1)
    };
}
