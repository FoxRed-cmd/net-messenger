using System.Security.Cryptography;
using System.Text;
using AuthService.Utils;

namespace AuthSrvice.Tests.Utils
{
    public class PasswordHasherTest
    {
        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordIsCorrect()
        {
            var password = Encoding.UTF8.GetBytes("MyStrongPassword123!");
            var salt = PasswordHasher.GenerateSalt();

            var expectedHash = PasswordHasher.HashPassword(password, salt, HashAlgorithmName.SHA256);

            Assert.True(PasswordHasher.VerifyPassword(password, salt, expectedHash, HashAlgorithmName.SHA256));
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            var password = Encoding.UTF8.GetBytes("MyStrongPassword123!");
            var incorrectPassword = Encoding.UTF8.GetBytes("MyPassword123!");
            var salt = PasswordHasher.GenerateSalt();

            var expectedHash = PasswordHasher.HashPassword(password, salt, HashAlgorithmName.SHA256);

            Assert.False(PasswordHasher.VerifyPassword(incorrectPassword, salt, expectedHash, HashAlgorithmName.SHA256));
        }
    }
}