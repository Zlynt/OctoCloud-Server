

using System.Security.Cryptography;
using System.Text;
using HashAlgo = System.Security.Cryptography.SHA256;

namespace OctoCloud.Server.Security
{
    public static class Hash
    {
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
    }
}