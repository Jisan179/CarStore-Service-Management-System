using System;
using System.Security.Cryptography;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Salted PBKDF2 password hashing. Stored format: PBKDF2$iterations$salt$hash
    /// </summary>
    public static class PasswordHelper
    {
        private const string Prefix = "PBKDF2$";
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static bool IsHashed(string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword) || !storedPassword.StartsWith(Prefix, StringComparison.Ordinal))
                return false;

            return storedPassword.Split('$').Length == 4;
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            byte[] salt = new byte[SaltSize];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, Iterations, HashSize);
            return Prefix + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || !IsHashed(storedPassword))
                return false;

            string[] parts = storedPassword.Split('$');
            if (!int.TryParse(parts[1], out int iterations) || iterations <= 0)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                byte[] actual = Derive(inputPassword, salt, iterations, expected.Length);

                // Constant-time comparison
                int difference = 0;
                for (int i = 0; i < expected.Length; i++)
                {
                    difference |= expected[i] ^ actual[i];
                }
                return difference == 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] Derive(string password, byte[] salt, int iterations, int length)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(length);
            }
        }
    }
}
