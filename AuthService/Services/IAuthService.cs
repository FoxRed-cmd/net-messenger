using AuthService.Entities.DTO;

namespace AuthService.Services
{
    public interface IAuthService : IDisposable
    {
        Task RegisterAsync(RegisterRequestDto registerRequestDto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
    }
}