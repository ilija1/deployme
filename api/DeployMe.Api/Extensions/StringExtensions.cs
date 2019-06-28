using System.Security.Cryptography;
using System.Text;

namespace DeployMe.Api.Extensions
{
    public static class StringExtensions
    {
        private static MD5 MD5 { get; } = MD5.Create();

        public static string ToMD5(this string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString().ToLower();
        }
    }
}
