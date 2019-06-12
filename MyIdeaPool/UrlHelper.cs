using System;
using System.Linq;
using System.Security.Cryptography;

namespace MyIdeaPool
{
    public static class UrlHelper
    {
        private static readonly string allowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";

        public static string GenerateRandomUrl(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] urlBytes = new byte[length];
                rng.GetBytes(urlBytes);

                // Distribute the bytes to each allowed character with equal probability.
                return new string(urlBytes.Select(b => allowedCharacters[b % allowedCharacters.Length]).ToArray());
            }

        }
    }
}
