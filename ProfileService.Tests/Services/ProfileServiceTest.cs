using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProfileService.Entities;
using ProfileService.Entities.DTO;
using ProfileService.Exceptions;
using ProfileService.Repositories;

namespace ProfileService.Tests.Services
{
    public class ProfileServiceTest
    {
        private readonly Mock<IServiceScopeFactory> scopeFactoryMock;
        private readonly Mock<IServiceScope> scopeMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IProfileRepository> profileRepositoryMock;
        private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
        private readonly ProfileService.Services.ProfileService profileService;

        public ProfileServiceTest()
        {
            scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeMock = new Mock<IServiceScope>();
            serviceProviderMock = new Mock<IServiceProvider>();
            profileRepositoryMock = new Mock<IProfileRepository>();
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // связываем scope и serviceProvider
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            // когда запрашиваем репозиторий — вернуть mock
            serviceProviderMock
                .Setup(p => p.GetService(typeof(IProfileRepository)))
                .Returns(profileRepositoryMock.Object);

            profileService = new ProfileService.Services.ProfileService(scopeFactoryMock.Object, httpContextAccessorMock.Object);
        }

        private void SetUser(Guid userId)
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            httpContextAccessorMock.Setup(a => a.HttpContext)
                                   .Returns(new DefaultHttpContext { User = principal });
        }

        [Fact]
        public async Task CreateProfileAsync_ShouldCreateProfile()
        {
            var dto = new CreateProfileDto { Email = "test@mail.com", FullName = "", UserName = "test" };
            profileRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Profile>())).Returns(Task.CompletedTask);
            profileRepositoryMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            await profileService.CreateProfileAsync(dto);

            profileRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Profile>()), Times.Once);
            profileRepositoryMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProfileAfterLogin_ShouldReturnProfile()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            profileRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                           .ReturnsAsync(new Profile
                           {
                               Id = userId,
                               FirstName = "Test",
                               UserName = "@test_user",
                               Email = "test@example.com"
                           });

            var result = await profileService.GetProfileAfterLogin();

            Assert.Equal("Test", result.FullName);
            Assert.Equal("@test_user", result.UserName);
            Assert.Equal("test@example.com", result.Email);

            profileRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetProfileAfterLogin_ShouldReturnProfile_WhenUserIdIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // создаём HttpContext с Claim
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            // настраиваем репозиторий
            var profile = new Profile
            {
                Id = userId,
                FirstName = "Test",
                UserName = "@test_user",
                Email = "test@example.com"
            };

            profileRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(profile);

            // Act
            var result = await profileService.GetProfileAfterLogin();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("@test_user", result.UserName);
            Assert.Equal("test@example.com", result.Email);

            profileRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetProfileAfterLogin_ShouldThrow_WhenUnauthorized()
        {
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(null as HttpContext);

            await Assert.ThrowsAsync<InvalidOperationException>(() => profileService.GetProfileAfterLogin());
        }

        [Fact]
        public async Task GetProfileByIdAsync_ShouldReturnProfile()
        {
            var id = Guid.NewGuid();
            profileRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(
                new Profile
                {
                    Id = id,
                    FirstName = "Test",
                    UserName = "@test_user",
                    Email = "test@example.com"
                });

            var result = await profileService.GetProfileByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Test", result.FullName);
            Assert.Equal("@test_user", result.UserName);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetProfileByIdAsync_ShouldThrow_WhenNotFound()
        {
            profileRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                           .ReturnsAsync(null as Profile);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                profileService.GetProfileByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task FindProfileAsync_ShouldReturnProfiles()
        {
            profileRepositoryMock.Setup(r => r.FindAsync("test"))
                           .ReturnsAsync(
                            [
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    FirstName = "Test",
                                    UserName = "u",
                                    Email = "test@example.com",
                                }
                            ]);

            var result = await profileService.FindProfileAsync("test");

            Assert.Single(result);
        }

        [Fact]
        public async Task FindProfileAsync_ShouldThrow_WhenQueryEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                profileService.FindProfileAsync(""));
        }

        [Fact]
        public async Task GetProfileByUserNameAsync_ShouldReturnProfile()
        {
            profileRepositoryMock.Setup(r => r.GetByUserNameAsync("@test_user"))
                           .ReturnsAsync(new Profile
                           {
                               Id = Guid.NewGuid(),
                               FirstName = "Test",
                               Email = "test@example.com",
                               UserName = "@test_user"
                           });

            var result = await profileService.GetProfileByUserNameAsync("@test_user");

            Assert.Equal("@test_user", result.UserName);
        }

        [Fact]
        public async Task GetProfileByUserNameAsync_ShouldThrow_WhenEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                profileService.GetProfileByUserNameAsync(""));
        }

        [Fact]
        public async Task GetProfileByEmailAsync_ShouldReturnProfile()
        {
            profileRepositoryMock.Setup(r => r.GetByEmailAsync("mail@test.com"))
                           .ReturnsAsync(new Profile
                           {
                               Id = Guid.NewGuid(),
                               FirstName = "Test",
                               Email = "mail@test.com",
                               UserName = "@test_user"
                           });

            var result = await profileService.GetProfileByEmailAsync("mail@test.com");

            Assert.Equal("mail@test.com", result.Email);
        }

        [Fact]
        public async Task GetProfileByEmailAsync_ShouldThrow_WhenEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                profileService.GetProfileByEmailAsync(""));
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateProfile()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            var dto = new UpdateProfileDto { UserName = "newUser", FirstName = "NewUser" };

            var existing = new Profile
            {
                Id = userId,
                FirstName = "OldUser",
                Email = "test@example.com",
                UserName = "oldUser"
            };

            profileRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existing);
            profileRepositoryMock.Setup(r => r.GetByUserNameAsync("newUser")).ReturnsAsync(null as Profile);

            await profileService.UpdateProfileAsync(dto);

            profileRepositoryMock.Verify(r => r.Update(existing), Times.Once);
            profileRepositoryMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldThrow_WhenUsernameTaken()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            var dto = new UpdateProfileDto { UserName = "existingUser", FirstName = "NewUser" };

            var existing = new Profile
            {
                Id = userId,
                FirstName = "OldUser",
                Email = "test@example.com",
                UserName = "oldUser"
            };

            profileRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existing);
            profileRepositoryMock.Setup(r => r.GetByUserNameAsync("existingUser"))
                           .ReturnsAsync(new Profile
                           {
                               Id = Guid.NewGuid(),
                               FirstName = "OldUser",
                               Email = "test@example.com",
                               UserName = "existingUser"
                           });

            await Assert.ThrowsAsync<EmailOrUsernameAlreadyExistsException>(() =>
                profileService.UpdateProfileAsync(dto));
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldThrow_WhenProfileNotFound()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            profileRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                           .ReturnsAsync(null as Profile);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                profileService.UpdateProfileAsync(new UpdateProfileDto()
                {
                    UserName = "newUser",
                    FirstName = "NewUser"
                }));
        }
    }
}