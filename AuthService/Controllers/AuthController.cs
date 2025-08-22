using AuthService.Entities.DTO;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService authService = authService;
        [HttpPost("register")]
        public async Task<IResult> Register(RegisterRequestDto registerRequestDto)
        {
            await authService.RegisterAsync(registerRequestDto);
            return Results.Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IResult> Login(LoginRequestDto loginRequestDto)
        {
            var loginResponse = await authService.LoginAsync(loginRequestDto);

            Response.Cookies.Append(nameof(loginResponse.RefreshToken).ToUpper(), loginResponse.RefreshToken, new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = loginResponse.RefreshTokenExpiration
            });

            return Results.Json(new { accessToken = loginResponse.AccessToken }, statusCode: 200);
        }

        [HttpPost("refresh")]
        public async Task<IResult> Refresh()
        {
            AuthResponseDto loginResponse;
            if (Request.Cookies.TryGetValue(nameof(loginResponse.RefreshToken).ToUpper(), out var refreshToken))
            {
                loginResponse = await authService.RefreshAsync(refreshToken);
                Response.Cookies.Append(nameof(loginResponse.RefreshToken).ToUpper(), loginResponse.RefreshToken, new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = loginResponse.RefreshTokenExpiration
                });

                return Results.Json(new { accessToken = loginResponse.AccessToken }, statusCode: 200);
            }

            return Results.Json(new
            {
                error = "Invalid refresh token provided",
                status = StatusCodes.Status401Unauthorized
            }, statusCode: 401);
        }
    }
}