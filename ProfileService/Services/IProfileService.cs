using ProfileService.Entities.DTO;

namespace ProfileService.Services
{
    public interface IProfileService
    {
        Task CreateProfileAsync(UserRegisteredEvent evt);
        Task<ProfileDto> GetProfileByIdAsync(Guid id);
        Task<ProfileDto> GetProfileByUserNameAsync(string userName);
        Task<IEnumerable<ProfileDto>> FindProfileAsync(string query);
    }
}