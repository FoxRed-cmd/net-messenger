using AuthService.Entities.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public IResult Register(RegisterRequestDto registerRequestDto)
        {
            return Results.Ok();
        }
    }
}