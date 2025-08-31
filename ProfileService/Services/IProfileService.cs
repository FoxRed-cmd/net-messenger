using ProfileService.Entities.DTO;

namespace ProfileService.Services
{
    public interface IProfileService
    {
        Task CreateProfileAsync(CreateProfileDto profileDto);
        Task<ProfileDto> GetProfileByIdAsync(Guid id);
        Task<ProfileDto> GetProfileByEmailAsync(string email);
        Task<ProfileDto> GetProfileByUserNameAsync(string userName);
        Task<IEnumerable<ProfileDto>> FindProfileAsync(string query);
        Task UpdateProfileAsync(UpdateProfileDto newProfileDto);
    }
}