using AuthService.Configs;
using AuthService.Entities;
using AuthService.Entities.DTO;
using AuthService.Entities.Enums;
using AuthService.Exceptions;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace AuthSrvice.Tests.Services
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserRepository> userRepositoryMock = new();
        private readonly Mock<IRabbitMqProducerService> producerServiceMock = new();
        private readonly Mock<IOptions<JwtSettings>> jwtSettingsMock = new();

        private readonly AuthService.Services.AuthService authService;

        public AuthServiceTest()
        {
            authService = new AuthService.Services.AuthService(userRepositoryMock.Object, producerServiceMock.Object, jwtSettingsMock.Object);
        }
        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenUserDoesNotExist()
        {
            var email = "test@example.com";
            var registerRequest = new RegisterRequestDto() { Email = email, Password = "hash_password" };

            userRepositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(null as User);
            userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(new User()
            {
                Id = Guid.NewGuid(),
                Email = email,
                Password = "hash_password",
                Salt = "hash_salt",
                Role = Role.USER,
                CreatedAt = DateTime.UtcNow
            });
            userRepositoryMock.Setup(r => r.SaveAsync()).ReturnsAsync(1);

            await authService.RegisterAsync(registerRequest);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenUserAlreadyExist()
        {
            var email = "test@example.com";
            var registerRequest = new RegisterRequestDto() { Email = email, Password = "hash_password" };

            userRepositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(new User() { Email = email });

            await Assert.ThrowsAsync<UserAlreadyExistException>(() => authService.RegisterAsync(registerRequest));
        }
    }
}