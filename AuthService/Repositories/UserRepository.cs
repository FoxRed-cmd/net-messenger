using AuthService.Data;
using AuthService.Entities;
using AuthService.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository(AuthDbContext context) : IUserRepository
    {
        private readonly AuthDbContext context = context;
        private bool disposed = false;
        public async Task<User> CreateAsync(User user)
        {
            var result = await context.Users.AddAsync(user);
            return result.Entity;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await context.Users.FindAsync(id) ?? throw new EntityNotFoundException("User not found");
        }

        public void Update(User user)
        {
            context.Entry(user).State = EntityState.Modified;
        }

        public Task<int> SaveAsync()
        {
            return context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}