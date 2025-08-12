using AuthService.Entities.DTO;
using AuthService.Repositories;

namespace AuthService.Services
{
    public class AuthService(IUserRepository userRepository) : IAuthService
    {
        private readonly IUserRepository userRepository = userRepository;
        private bool disposed = false;
        public Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            throw new NotImplementedException();
        }

        public Task RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            throw new NotImplementedException();
        }

        public void Dispose(bool disposing)
        {
            if (!disposed)
                if (disposing)
                    userRepository.Dispose();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}