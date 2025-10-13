using System.Security.Cryptography;
namespace PeShop.Extensions
{
    public static class GenerateRandomKeyExtension
    {
        public static string GenerateRandomKey(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = chars[bytes[i] % chars.Length];
            return new string(result);
        }
    }
}