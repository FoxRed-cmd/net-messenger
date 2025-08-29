using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProfileService.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController(ProfileService.Services.IProfileService profileService)
    {
        private readonly ProfileService.Services.IProfileService profileService = profileService;

        [Authorize]
        [HttpGet("find")]
        public async Task<IResult> Find([FromQuery] string query)
        {
            var profiles = await profileService.FindProfileAsync(query);
            return Results.Json(profiles, statusCode: 200);
        }
    }
}