using ProfileService.Entities;

namespace ProfileService.Repositories
{
    public interface IProfileRepository : ICrudRepository<Profile, Guid>
    {

    }
}