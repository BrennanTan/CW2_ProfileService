using System.Security.Cryptography;

namespace CW2_ProfileService
{
    public class PasswordHasher
    {
        private const int saltSize = 16;
        private const int keySize = 32;
        private const int iterations = 10000;
        private static readonly HashAlgorithmName hashAlgorithmName = HashAlgorithmName.SHA256;
        private static char Delimiter = ';';
        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(saltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithmName, keySize);

            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool Verify(string passwordHash, string inputPassword)
        {
            var elements = passwordHash.Split(Delimiter);
            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(inputPassword, salt, iterations, hashAlgorithmName, keySize);

            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }
}
