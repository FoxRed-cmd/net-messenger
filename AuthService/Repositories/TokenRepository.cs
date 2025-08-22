using AuthService.Data;
using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class TokenRepository(AuthDbContext context) : ITokenRepository
    {
        private readonly AuthDbContext context = context;
        public async Task<RefreshToken> CreateAsync(RefreshToken token)
        {
            await context.RefreshTokens.AddAsync(token);
            return token;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await context.RefreshTokens.FindAsync(token);
        }

        public Task<int> SaveAsync()
        {
            return context.SaveChangesAsync();
        }

        public void Update(RefreshToken token)
        {
            context.Entry(token).State = EntityState.Modified;
        }
    }
}