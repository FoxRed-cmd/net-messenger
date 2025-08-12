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
    }
}