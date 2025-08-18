using System.Security.Cryptography;

namespace AuthService.Utils
{
    public static class PasswordHasher
    {
        private const int SALT_SIZE = 16; // 128 бит
        private const int KEY_SIZE = 32;  // 256 бит
        private const int ITERATIONS = 100_000;

        public static byte[] HashPassword(byte[] password, byte[] salt, HashAlgorithmName algorithm)
        {
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, algorithm, KEY_SIZE);
        }

        public static (string, string) HashPassword(byte[] password, HashAlgorithmName algorithm)
        {
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt, algorithm);

            Array.Clear(password, 0, password.Length);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static byte[] GenerateSalt()
        {
            var salt = new byte[SALT_SIZE];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        public static bool VerifyPassword(byte[] password, byte[] salt, byte[] expectedHash, HashAlgorithmName algorithm)
        {
            var hash = HashPassword(password, salt, algorithm);
            return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
        }
    }
}