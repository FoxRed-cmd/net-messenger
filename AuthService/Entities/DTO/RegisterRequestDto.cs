using System.ComponentModel.DataAnnotations;

namespace AuthService.Entities.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; init; }
        [Required]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
        public required string Password { get; init; }
    }
}