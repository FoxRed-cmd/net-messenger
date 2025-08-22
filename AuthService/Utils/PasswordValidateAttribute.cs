using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AuthService.Utils
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class PasswordValidateAttribute : ValidationAttribute
    {
        private readonly Regex regex;
        public PasswordValidateAttribute(string pattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$")
        {
            regex = new Regex(pattern, RegexOptions.Compiled);
            ErrorMessage =
            "The password must contain at least one uppercase letter, one number, one special character and be at least 8 characters long";
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string password)
            {
                if (string.IsNullOrEmpty(password))
                    return new ValidationResult("Password is required");

                return regex.IsMatch(password) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
            }

            return new ValidationResult("Password is required");

        }
    }
}