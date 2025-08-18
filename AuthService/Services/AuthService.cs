using System.Security.Cryptography;
using System.Text;
using AuthService.Entities;
using AuthService.Entities.DTO;
using AuthService.Entities.Enums;
using AuthService.Exceptions;
using AuthService.Repositories;
using AuthService.Utils;

namespace AuthService.Services
{
    public class AuthService(IUserRepository userRepository, IRabbitMqProducerService rabbitMqProducerService) : IAuthService
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IRabbitMqProducerService rabbitMqProducerService = rabbitMqProducerService;
        private bool disposed = false;
        public Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            var user = await userRepository.GetByEmailAsync(registerRequestDto.Email);
            if (user != null)
                throw new UserAlreadyExistException("User already exist");

            var (hashBase64, saltBase64) = PasswordHasher.HashPassword(
                Encoding.UTF8.GetBytes(registerRequestDto.Password),
                HashAlgorithmName.SHA256
            );

            user = new User()
            {
                Id = Guid.NewGuid(),
                Email = registerRequestDto.Email,
                Password = hashBase64,
                Salt = saltBase64,
                Role = Role.USER,
                CreatedAt = DateTime.UtcNow
            };

            await userRepository.CreateAsync(user);

            await userRepository.SaveAsync();

            await rabbitMqProducerService.PublishUserRegisteredAsync(new UserRegisteredEvent()
            {
                Id = user.Id,
                Email = user.Email
            });
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