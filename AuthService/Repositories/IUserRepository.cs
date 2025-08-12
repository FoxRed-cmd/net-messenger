using AuthService.Entities;

namespace AuthService.Repositories
{
    public interface IUserRepository : IDisposable
    {
        Task<User> CreateAsync(User user);
        void Update(User user);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(Guid id);
        Task<int> SaveAsync();
    }
}