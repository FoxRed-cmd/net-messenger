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

            return new ProfileDto()
            {
                Id = profile.Id,
                FullName = profile.FullName.Trim(),
                Email = profile.Email,
                UserName = profile.UserName,
                AvatarUrl = profile.AvatarUrl,
                Status = profile.Status,
                CreatedAt = profile.CreatedAt,
                LastVisit = profile.LastVisit,
                StatusOnline = profile.StatusOnline,
            };
        }

        public async Task<IEnumerable<ProfileDto>> FindProfileAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new Exception("Query is empty");

            using var scope = scopeFactory.CreateScope();
            var profileRepository = scope.ServiceProvider.GetRequiredService<IProfileRepository>();

            var profiles = await profileRepository.FindAsync(query);

            return [.. profiles.Select(p => new ProfileDto()
            {
                Id = p.Id,
                FullName = p.FullName.Trim(),
                Email = p.Email,
                UserName = p.UserName,
                AvatarUrl = p.AvatarUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                LastVisit = p.LastVisit,
                StatusOnline = p.StatusOnline,
            })];
        }
    }
}