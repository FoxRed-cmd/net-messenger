using ProfileService.Entities.DTO;

namespace ProfileService.Services
{
    public interface IProfileService
    {
        Task CreateProfileAsync(UserRegisteredEvent evt);
    }
}