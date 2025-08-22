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
    public class AuthService(
        IUserRepository userRepository,
        ITokenRepository tokenRepository,
        IRabbitMqProducerService rabbitMqProducerService,
        IOptions<JwtSettings> jwtOptions) : IAuthService
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly ITokenRepository tokenRepository = tokenRepository;
        private readonly IRabbitMqProducerService rabbitMqProducerService = rabbitMqProducerService;
        private readonly IOptions<JwtSettings> jwtOptions = jwtOptions;
        private bool disposed = false;
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await userRepository.GetByEmailAsync(loginRequestDto.Email) ??
                throw new IncorrectLoginOrPasswordException("Incorrect login or password");

            if (!PasswordHasher.VerifyPassword(loginRequestDto.Password, user.Salt, user.Password, HashAlgorithmName.SHA256))
                throw new IncorrectLoginOrPasswordException("Incorrect login or password");

            var refreshToken = await tokenRepository.CreateAsync(GenerateRefreshToken(user));
            await tokenRepository.SaveAsync();


            return new AuthResponseDto()
            {
                AccessToken = GenerateJwtToken(user),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.Expires
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

        public async Task<AuthResponseDto> RefreshAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new SecurityTokenException("No refresh token provided");

            var refreshToken = await tokenRepository.GetByTokenAsync(token) ??
                throw new SecurityTokenException("Invalid refresh token provided");

            if (refreshToken.IsUsed || refreshToken.IsRevoked)
                throw new SecurityTokenException("Invalid refresh is used or revoked");

            if (refreshToken.Expires < DateTime.UtcNow)
                throw new SecurityTokenException("Refresh token expired");

            var user = refreshToken.User ??
                throw new SecurityTokenException("Invalid refresh token provided");

            refreshToken.IsUsed = true;
            tokenRepository.Update(refreshToken);

            refreshToken = await tokenRepository.CreateAsync(GenerateRefreshToken(user));
            await tokenRepository.SaveAsync();

            return new AuthResponseDto()
            {
                AccessToken = GenerateJwtToken(user),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.Expires
            };
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

        private RefreshToken GenerateRefreshToken(User user)
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                // TODO: add getting expires from config
                Expires = DateTime.UtcNow.AddDays(30),
                IsRevoked = false,
                IsUsed = false
            };

            return refreshToken;
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