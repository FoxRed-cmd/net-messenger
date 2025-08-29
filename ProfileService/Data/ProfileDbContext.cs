using Microsoft.EntityFrameworkCore;
using ProfileService.Entities;

namespace ProfileService.Data
{
    public class ProfileDbContext(DbContextOptions<ProfileDbContext> options, IConfiguration configuration) : DbContext(options)
    {
        public DbSet<Profile> Profiles { get; set; }
        private readonly IConfiguration configuration = configuration;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(configuration.GetConnectionString("ProfileDb"));
        }
    }
}