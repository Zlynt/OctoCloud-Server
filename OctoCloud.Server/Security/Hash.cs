

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using HashAlgo = System.Security.Cryptography.SHA256;

namespace OctoCloud.Server.Security
{
    public static class Hash
    {
        private const int SaltSize = 128 / 8;
        private const int KeySize = 256 / 8;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName PasswordHashAlgorithmName = HashAlgorithmName.SHA256;
        private static char Delimiter = ';';

        public static string GetHash(string text){
            using (HashAlgo sha256Hash = HashAlgo.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string CreatePasswordHash(string password){
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, PasswordHashAlgorithmName, KeySize);

            return string.Join(
                Delimiter, 
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash)
            );
        }
        public static bool VerifyPasswordHash(string password, string passwordHash){
            string[] saltAndHash = passwordHash.Split(';');
            byte[] salt = Convert.FromBase64String(saltAndHash[0]);
            byte[] hash = Convert.FromBase64String(saltAndHash[1]);

            byte[] calculatedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, PasswordHashAlgorithmName, KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, calculatedHash);
        }
    }
}