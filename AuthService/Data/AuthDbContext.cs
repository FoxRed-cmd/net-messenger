using AuthService.Entities;
using AuthService.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        private readonly IConfiguration configuration = configuration;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseNpgsql(configuration.GetConnectionString("AuthDb"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .Property(u => u.Role)
                .HasConversion(r => r.ToString(), r => Enum.Parse<Role>(r));
        }
    }
}