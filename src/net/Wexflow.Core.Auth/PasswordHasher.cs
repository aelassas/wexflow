using System;
using System.Security.Cryptography;

namespace Wexflow.Core.Auth
{
    /// <summary>
    /// PBKDF2 Password Hasher.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128-bit
        private const int HashSize = 32; // 256-bit
        private const int Iterations = 100_000;

        public static string HashPassword(string password)
        {
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            var combined = new byte[1 + SaltSize + HashSize];
            combined[0] = 0x01; // version byte
            Buffer.BlockCopy(salt, 0, combined, 1, SaltSize);
            Buffer.BlockCopy(hash, 0, combined, 1 + SaltSize, HashSize);

            return Convert.ToBase64String(combined);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(storedHash);
            }
            catch
            {
                return false;
            }

            if (decoded.Length != 1 + SaltSize + HashSize || decoded[0] != 0x01)
            {
                return false; // unsupported version or invalid format
            }

            var salt = new byte[SaltSize];
            var storedSubkey = new byte[HashSize];

            Buffer.BlockCopy(decoded, 1, salt, 0, SaltSize);
            Buffer.BlockCopy(decoded, 1 + SaltSize, storedSubkey, 0, HashSize);

            byte[] computedSubkey;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                computedSubkey = pbkdf2.GetBytes(HashSize);
            }

            return FixedTimeEquals(storedSubkey, computedSubkey);
        }

        // Constant-time comparison
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}
