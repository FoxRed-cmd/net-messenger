using ProfileService.Entities;
using ProfileService.Entities.DTO;
using ProfileService.Repositories;

namespace ProfileService.Services
{
    public class ProfileService(IServiceScopeFactory scopeFactory) : IProfileService
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        public async Task CreateProfileAsync(UserRegisteredEvent evt)
        {
            using var scope = scopeFactory.CreateScope();
            var profileRepository = scope.ServiceProvider.GetRequiredService<IProfileRepository>();

            var profile = new Profile()
            {
                Id = evt!.Id,
                DisplayName = evt.Email,
                AvatarUrl = null,
                Status = null
            };

            await profileRepository.CreateAsync(profile);
            await profileRepository.SaveAsync();
        }
    }
}