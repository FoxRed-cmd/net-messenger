using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Configs;
using AuthService.Entities;
using AuthService.Entities.DTO;
using AuthService.Entities.Enums;
using AuthService.Exceptions;
using AuthService.Repositories;
using AuthService.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public class AuthService(IUserRepository userRepository, IRabbitMqProducerService rabbitMqProducerService, IOptions<JwtSettings> jwtOptions) : IAuthService
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IRabbitMqProducerService rabbitMqProducerService = rabbitMqProducerService;
        private readonly IOptions<JwtSettings> jwtOptions = jwtOptions;
        private bool disposed = false;
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await userRepository.GetByEmailAsync(loginRequestDto.Email) ??
                throw new IncorrectLoginOrPasswordException("Incorrect login or password");

            if (!PasswordHasher.VerifyPassword(loginRequestDto.Password, user.Salt, user.Password, HashAlgorithmName.SHA256))
                throw new IncorrectLoginOrPasswordException("Incorrect login or password");

            // TODO: add logic for refresh token
            return new AuthResponseDto()
            {
                AccessToken = GenerateJwtToken(user),
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };
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

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var creds = new SigningCredentials(
                jwtOptions.Value.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Value.Issuer,
                audience: jwtOptions.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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