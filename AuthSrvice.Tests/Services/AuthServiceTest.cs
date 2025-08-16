using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Entities;
using AuthService.Entities.DTO;
using AuthService.Exceptions;
using AuthService.Repositories;
using AuthService.Services;
using Moq;
using Xunit;

namespace AuthSrvice.Tests.Services
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserRepository> userRepositoryMock = new();
        private readonly Mock<IRabbitMqProducerService> producerServiceMock = new();

        private readonly AuthService.Services.AuthService authService;

        public AuthServiceTest()
        {
            authService = new AuthService.Services.AuthService(userRepositoryMock.Object, producerServiceMock.Object);
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