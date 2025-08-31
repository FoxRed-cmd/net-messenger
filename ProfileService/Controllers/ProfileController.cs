using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Entities.DTO;

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

        [Authorize]
        [HttpGet("login")]
        public async Task<IResult> GetProfileAfterLogin()
        {
            var profile = await profileService.GetProfileAfterLogin();
            return Results.Json(profile, statusCode: 200);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IResult> UpdateProfile([FromBody] UpdateProfileDto profileDto)
        {
            await profileService.UpdateProfileAsync(profileDto);
            return Results.Ok("Profile updated successfully");
        }
    }
}