using ProfileService.Entities;

namespace ProfileService.Repositories
{
    public interface IProfileRepository : ICrudRepository<Profile, Guid>
    {
        Task<Profile?> GetByEmailAsync(string email);
        Task<Profile?> GetByUserNameAsync(string userName);
        Task<Profile?> GetByFirstNameAsync(string firstName);
        Task<IEnumerable<Profile>> FindAsync(string query);
    }
}