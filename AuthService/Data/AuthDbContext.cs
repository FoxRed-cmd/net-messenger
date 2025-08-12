using AuthService.Entities;
using AuthService.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(r => r.ToString(), r => Enum.Parse<Role>(r));
        }
    }
}