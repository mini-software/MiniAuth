using System;
using System.IO;
using System.Security.Cryptography;
namespace MiniAuth.Helpers
{
    internal class HashGenerator
    {
        private const string SaltFilePath = "miniauthsalt.cer";
        public static string GetHashPassword(string password)
        {
            byte[] salt = LoadOrCreateSalt();
            byte[] hash = HashPassword(password, salt);
            return Convert.ToBase64String(hash);
        }
        private static byte[] LoadOrCreateSalt()
        {
            byte[] salt;

            if (File.Exists(SaltFilePath))
            {
                salt = File.ReadAllBytes(SaltFilePath);
            }
            else
            {
                salt = GenerateSalt();
                File.WriteAllBytes(SaltFilePath, salt);
            }
            return salt;
        }
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return salt;
        }
        private static byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(32);
        }
    }
}
