using System.ComponentModel.DataAnnotations;
using AuthService.Utils;

namespace AuthService.Entities.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; init; }

        [PasswordValidate]
        public required string Password { get; init; }
    }
}