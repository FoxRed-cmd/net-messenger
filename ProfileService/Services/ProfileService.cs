using Mapster;
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

            var tempName = string.Format("user_{0}", Random.Shared.Next(0, int.MaxValue));
            var halfMails = evt.Email.Split("@");
            var tempUserName = $"@{halfMails.First()}_{halfMails.Last()}";

            await profileRepository.CreateAsync(new Profile()
            {
                Id = evt!.Id,
                FirstName = tempName,
                LastName = null,
                Email = evt.Email,
                UserName = tempUserName,
                AvatarUrl = null,
                Status = null,
                CreatedAt = DateTime.UtcNow,
                LastVisit = DateTime.UtcNow,
                StatusOnline = true
            });
            await profileRepository.SaveAsync();
        }

        public async Task<ProfileDto> GetProfileByIdAsync(Guid id)
        {
            using var scope = scopeFactory.CreateScope();
            var profileRepository = scope.ServiceProvider.GetRequiredService<IProfileRepository>();

            var profile = await profileRepository.GetByIdAsync(id) ?? throw new Exception("Profile not found");

            return profile.Adapt<ProfileDto>();
        }

        public async Task<IEnumerable<ProfileDto>> FindProfileAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new Exception("Query is empty");

            using var scope = scopeFactory.CreateScope();
            var profileRepository = scope.ServiceProvider.GetRequiredService<IProfileRepository>();

            var profiles = await profileRepository.FindAsync(query);

            return [.. profiles.Select(p => p.Adapt<ProfileDto>())];
        }

        public async Task<ProfileDto> GetProfileByUserNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new Exception("Username is empty");

            using var scope = scopeFactory.CreateScope();
            var profileRepository = scope.ServiceProvider.GetRequiredService<IProfileRepository>();

            var profile = await profileRepository.GetByUserNameAsync(userName);

            return profile.Adapt<ProfileDto>();
        }
    }
}