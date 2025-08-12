using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public IResult Register()
        {
            return Results.Ok("User registered successfully");
        }
    }
}