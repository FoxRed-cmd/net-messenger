using AuthService.Entities;

namespace AuthService.Repositories
{
    public interface ITokenRepository
    {
        Task<RefreshToken> CreateAsync(RefreshToken token);
        void Update(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<int> SaveAsync();
    }
}