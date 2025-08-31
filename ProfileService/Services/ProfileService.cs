using System.Security.Claims;
using Mapster;
using ProfileService.Entities;
using ProfileService.Entities.DTO;
using ProfileService.Repositories;

namespace ProfileService.Services
{
    public class ProfileService(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor) : IProfileService
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private IServiceScope? scope;
        public async Task CreateProfileAsync(CreateProfileDto profileDto)
        {
            var profileRepository = GetProfileRepository();

            profileDto.FullName = string.Format("user_{0}", Random.Shared.Next(0, int.MaxValue));
            var halfMails = profileDto.Email.Split("@");
            profileDto.UserName = $"@{halfMails.First()}_{halfMails.Last()}";
            profileDto.CreatedAt = DateTime.UtcNow;
            profileDto.LastVisit = DateTime.UtcNow;
            profileDto.StatusOnline = true;

            await profileRepository.CreateAsync(profileDto.Adapt<Profile>());
            await profileRepository.SaveAsync();

            scope?.Dispose();
        }

        public async Task<ProfileDto> GetProfileAfterLogin()
        {
            var userId = GetUserId();
            return await GetProfileByIdAsync(userId);
        }

        public async Task<ProfileDto> GetProfileByIdAsync(Guid id)
        {
            var profileRepository = GetProfileRepository();

            var profile = await profileRepository.GetByIdAsync(id) ?? throw new Exception("Profile not found");

            scope?.Dispose();

            return profile.Adapt<ProfileDto>();
        }

        public async Task<IEnumerable<ProfileDto>> FindProfileAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new Exception("Query is empty");

            var profileRepository = GetProfileRepository();

            var profiles = await profileRepository.FindAsync(query);

            scope?.Dispose();

            return [.. profiles.Select(p => p.Adapt<ProfileDto>())];
        }

        public async Task<ProfileDto> GetProfileByUserNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new Exception("Username is empty");

            var profileRepository = GetProfileRepository();

            var profile = await profileRepository.GetByUserNameAsync(userName);

            scope?.Dispose();

            return profile.Adapt<ProfileDto>();
        }

        public async Task<ProfileDto> GetProfileByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new Exception("Email is empty");

            var profileRepository = GetProfileRepository();

            var profile = await profileRepository.GetByEmailAsync(email);

            scope?.Dispose();

            return profile.Adapt<ProfileDto>();
        }

        public async Task UpdateProfileAsync(UpdateProfileDto newProfileDto)
        {
            var profileRepository = GetProfileRepository();

            var userId = GetUserId();

            var profile = await profileRepository.GetByIdAsync(userId)
                ?? throw new Exception("Profile not found");

            if (!profile.Email.Equals(newProfileDto.Email))
            {
                var emailExists = await profileRepository.GetByEmailAsync(newProfileDto.Email) != null;
                if (emailExists)
                    throw new Exception("Email already exists");
            }

            if (!profile.UserName.Equals(newProfileDto.UserName))
            {
                var userNameExists = await profileRepository.GetByUserNameAsync(newProfileDto.UserName) != null;
                if (userNameExists)
                    throw new Exception("Username already exists");
            }

            newProfileDto.Adapt(profile);

            profileRepository.Update(profile);
            await profileRepository.SaveAsync();

            scope?.Dispose();
        }

        private IProfileRepository GetProfileRepository()
        {
            scope = scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IProfileRepository>();
        }

        private Guid GetUserId()
        {
            if (httpContextAccessor is not null && httpContextAccessor.HttpContext is not null)
            {
                var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException();

                return Guid.Parse(userId);
            }
            throw new InvalidOperationException("HttpContextAccessor is not set");
        }
    }
}