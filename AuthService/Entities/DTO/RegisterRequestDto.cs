namespace AuthService.Entities.DTO
{
    public class RegisterRequestDto
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}