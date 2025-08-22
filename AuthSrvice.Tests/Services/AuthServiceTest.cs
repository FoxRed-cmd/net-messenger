using System.Security.Cryptography;
using System.Text;
using AuthService.Configs;
using AuthService.Entities;
using AuthService.Entities.DTO;
using AuthService.Entities.Enums;
using AuthService.Exceptions;
using AuthService.Repositories;
using AuthService.Services;
using AuthService.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AuthSrvice.Tests.Services
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserRepository> userRepositoryMock = new();
        private readonly Mock<ITokenRepository> tokenRepositoryMock = new();
        private readonly Mock<IRabbitMqProducerService> producerServiceMock = new();
        private readonly Mock<IOptions<JwtSettings>> jwtSettingsMock = new();

        private readonly AuthService.Services.AuthService authService;

        public AuthServiceTest()
        {
            authService = new AuthService.Services.AuthService(
                userRepositoryMock.Object,
                tokenRepositoryMock.Object,
                producerServiceMock.Object,
                jwtSettingsMock.Object);
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

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
        {
            var email = "test@example.com";
            var loginRequest = new LoginRequestDto() { Email = email, Password = "hash_password" };

            userRepositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(null as User);

            await Assert.ThrowsAsync<IncorrectLoginOrPasswordException>(() => authService.LoginAsync(loginRequest));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenPasswordIsIncorrect()
        {
            var email = "test@example.com";
            var salt = PasswordHasher.GenerateSalt();
            var correctPassword = "correct_password";
            var incorrectPassword = "incorrect_input_password";

            var hashPassword = PasswordHasher.HashPassword(Encoding.UTF8.GetBytes(correctPassword), salt, HashAlgorithmName.SHA256);

            var loginRequest = new LoginRequestDto() { Email = email, Password = incorrectPassword };

            userRepositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(new User()
            {
                Email = email,
                Salt = Convert.ToBase64String(salt),
                Password = Convert.ToBase64String(hashPassword)
            });

            await Assert.ThrowsAsync<IncorrectLoginOrPasswordException>(() => authService.LoginAsync(loginRequest));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenPasswordIsCorrect()
        {
            var email = "test@example.com";
            var salt = PasswordHasher.GenerateSalt();
            var correctPassword = "correct_password";

            var hashPassword = PasswordHasher.HashPassword(Encoding.UTF8.GetBytes(correctPassword), salt, HashAlgorithmName.SHA256);

            var loginRequest = new LoginRequestDto() { Email = email, Password = correctPassword };

            userRepositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(new User()
            {
                Id = Guid.NewGuid(),
                Email = email,
                Salt = Convert.ToBase64String(salt),
                Password = Convert.ToBase64String(hashPassword),
                Role = Role.USER,
                CreatedAt = DateTime.UtcNow
            });
            jwtSettingsMock.Setup(j => j.Value).Returns(new JwtSettings()
            {
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                SecretKey = "supersecretkey1234567890_supersecretkey1234567890",
                ExpiryMinutes = 30
            });
            tokenRepositoryMock.Setup(t => t.CreateAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync(new RefreshToken
                {
                    Token = "refresh-token",
                    Expires = DateTime.UtcNow.AddDays(30)
                });

            var authResponse = await authService.LoginAsync(loginRequest);

            Assert.False(string.IsNullOrEmpty(authResponse.AccessToken));
            Assert.False(string.IsNullOrEmpty(authResponse.RefreshToken));
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenIsInvalid()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(null as RefreshToken);

            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RefreshAsync(refreshToken));
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenIsEmpty()
        {
            var refreshToken = string.Empty;

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(null as RefreshToken);

            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RefreshAsync(refreshToken));
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenWithUserIsNull()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var user = new User() { Id = Guid.NewGuid(), Email = "test@example.com" };

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(new RefreshToken()
            {
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(-30),
                User = null
            });

            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RefreshAsync(refreshToken));
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenIsExpired()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var user = new User() { Id = Guid.NewGuid(), Email = "test@example.com" };

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(new RefreshToken()
            {
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(-30),
                User = user
            });

            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RefreshAsync(refreshToken));
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenIsUsedOrRevoked()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var user = new User() { Id = Guid.NewGuid(), Email = "test@example.com" };

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(new RefreshToken()
            {
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = true,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(30),
                User = user
            });

            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RefreshAsync(refreshToken));

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(new RefreshToken()
            {
                Token = refreshToken,
                IsUsed = true,
                IsRevoked = false,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(30),
                User = user
            });

            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RefreshAsync(refreshToken));

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(new RefreshToken()
            {
                Token = refreshToken,
                IsUsed = true,
                IsRevoked = true,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(30),
                User = user
            });

            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RefreshAsync(refreshToken));
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnRefreshToken_WhenRefreshTokenIsValid()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var user = new User() { Id = Guid.NewGuid(), Email = "test@example.com" };

            tokenRepositoryMock.Setup(t => t.GetByTokenAsync(refreshToken)).ReturnsAsync(new RefreshToken()
            {
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(30),
                User = user
            });
            jwtSettingsMock.Setup(j => j.Value).Returns(new JwtSettings()
            {
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                SecretKey = "supersecretkey1234567890_supersecretkey1234567890",
                ExpiryMinutes = 30
            });
            tokenRepositoryMock.Setup(t => t.CreateAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync(new RefreshToken
                {
                    Token = "refresh-token",
                    Expires = DateTime.UtcNow.AddDays(30)
                });

            var result = await authService.RefreshAsync(refreshToken);

            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }
    }
}