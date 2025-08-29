using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.Entities;

namespace ProfileService.Repositories
{
    public class ProfileRepository(ProfileDbContext context) : CrudRepository<Profile, Guid>(context), IProfileRepository
    {
    }
}