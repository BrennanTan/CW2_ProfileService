using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace CW2_ProfileService
{
    public class PasswordHasher
    {
        private const int saltSize = 16;
        private const int hashSize = 32;
        private const int iterations = 4; // Adjust as needed for performance vs. security tradeoff
        private static char Delimiter = ';';
        public string Hash(string password)
        {
            var salt = new byte[saltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8; // Adjust based on system capabilities
                argon2.MemorySize = 65536; // Adjust based on system capabilities
                argon2.Iterations = iterations;

                var hash = argon2.GetBytes(hashSize);
                return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
            }
        }

        public bool Verify(string passwordHash, string inputPassword)
        {
            var elements = passwordHash.Split(Delimiter);
            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(inputPassword)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8; // Adjust based on system capabilities
                argon2.MemorySize = 65536; // Adjust based on system capabilities
                argon2.Iterations = iterations;

                var hashInput = argon2.GetBytes(hashSize);
                return CryptographicOperations.FixedTimeEquals(hash, hashInput);
            }
        }
    }
}
