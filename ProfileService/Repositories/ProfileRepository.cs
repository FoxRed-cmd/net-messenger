using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.Entities;

namespace ProfileService.Repositories
{
    public class ProfileRepository(ProfileDbContext context) : CrudRepository<Profile, Guid>(context), IProfileRepository
    {
        private readonly ProfileDbContext context = context;
        public async Task<Profile?> GetByEmailAsync(string email)
        {
            return await context.Profiles.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Profile?> GetByFirstNameAsync(string firstName)
        {
            return await context.Profiles.FirstOrDefaultAsync(p => p.FirstName == firstName);
        }

        public async Task<Profile?> GetByUserNameAsync(string userName)
        {
            return await context.Profiles.FirstOrDefaultAsync(p => p.UserName == userName);
        }

        public async Task<IEnumerable<Profile>> FindAsync(string query)
        {
            return await context.Profiles
                .Where(p => EF.Functions.ILike(p.Email, $"%{query}%") ||
                    EF.Functions.ILike(p.FirstName, $"%{query}%") ||
                    EF.Functions.ILike(p.LastName ?? string.Empty, $"%{query}%") ||
                    EF.Functions.ILike(p.UserName, $"%{query}%")).ToListAsync();
        }
    }
}