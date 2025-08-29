using ProfileService.Entities.DTO;

namespace ProfileService.Services
{
    public interface IProfileService
    {
        Task CreateProfileAsync(UserRegisteredEvent evt);
        Task<ProfileDto> GetProfileByIdAsync(Guid id);
        Task<IEnumerable<ProfileDto>> FindProfileAsync(string query);
    }
}