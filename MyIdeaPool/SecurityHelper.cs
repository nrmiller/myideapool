using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MyIdeaPool
{
    public static class SecurityHelper
    {
        private static readonly int saltLength = 32;
        private static readonly int hashLength = 32;
        private static readonly int hashIterations = 1000;

        public static byte[] GenerateSalt()
        {
            byte[] saltBytes = new byte[saltLength];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
                return saltBytes;
            }
        }

        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).ToLower().Replace("-", string.Empty);
        }

        public static byte[] ToBytes(string hex)
        {
            int numBytes = hex.Length / 2;
            byte[] bytes = new byte[numBytes];

            for (int index = 0; index < hex.Length; index += 2)
            {
                bytes[index / 2] = Convert.ToByte(hex.Substring(index, 2), 16);
            }
            return bytes;
        }

        public static byte[] GenerateSaltedHash(byte[] password, byte[] salt)
        {
            byte[] saltedPassword = password.Concat(salt).ToArray();
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, hashIterations))
            {
                return pbkdf2.GetBytes(hashLength);
            }
        }

        public static bool VerifyPassword(byte[] password, byte[] salt, byte[] saltedHash)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, hashIterations))
            {
                byte[] computedHash = pbkdf2.GetBytes(hashLength);

                // If the computed hash equals the stored hash, then the provided
                // password is verified.
                return computedHash.SequenceEqual(saltedHash);
            }
        }
    }
}
