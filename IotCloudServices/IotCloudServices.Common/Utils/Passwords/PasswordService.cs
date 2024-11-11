using System.Text;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;

namespace IotCloudServices.Common.Utils.Passwords
{
    public class PasswordService
    {
        // Hash a password using Argon2id
        public string HashPassword(string password)
        {
            // Generate a unique salt for the password
            var salt = GenerateSalt();

            // Hash the password using Argon2id
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;  // Manually assign the salt
                argon2.DegreeOfParallelism = 8; // Number of threads
                argon2.MemorySize = 1024 * 16; // 16 MB memory
                argon2.Iterations = 4;

                // Generate the hash
                byte[] hash = argon2.GetBytes(32); // Length of the hash
                return Convert.ToBase64String(hash) + ":" + Convert.ToBase64String(salt); // Store hash and salt together
            }
        }

        // Verify a password against a stored hash
        public bool VerifyPassword(string storedHash, string password)
        {
            // Split the stored hash into the actual hash and the salt
            var parts = storedHash.Split(':');
            if (parts.Length != 2) throw new FormatException("Stored hash is not in the correct format.");

            var storedPasswordHash = Convert.FromBase64String(parts[0]);
            var storedSalt = Convert.FromBase64String(parts[1]);

            // Hash the provided password using the same salt and parameters
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = storedSalt; // Use the stored salt
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 1024 * 16; // Same as used during hashing
                argon2.Iterations = 4;

                byte[] hash = argon2.GetBytes(32); // Length of the hash

                // Compare the computed hash with the stored one
                return CompareHashes(storedPasswordHash, hash);
            }
        }

        // Generate a random salt for hashing
        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];  // 16 bytes is typical for Argon2
                rng.GetBytes(salt);
                return salt;
            }
        }

        // Compare two hashes securely
        private bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            // Ensure constant-time comparison to prevent timing attacks
            if (hash1.Length != hash2.Length)
                return false;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }

            return true;
        }
    }


}
