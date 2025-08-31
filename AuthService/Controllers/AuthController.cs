using System.Threading.Tasks;
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

            return Results.Json(new { accessToken = loginResponse.AccessToken }, statusCode: 200);
        }

        [HttpPost("refresh")]
        public async Task<IResult> Refresh()
        {
            var loginResponse = await authService.RefreshAsync();

            return Results.Json(new { accessToken = loginResponse.AccessToken }, statusCode: 200);
        }

        [HttpPost("logout")]
        public async Task<IResult> Logout()
        {
            await authService.LogoutAsync();
            return Results.Ok("User logged out successfully");
        }
    }
}