using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly:InternalsVisibleTo("SudokuCollective.Data")]
[assembly:InternalsVisibleTo("SudokuCollective.Repos")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Encrypt
{
    internal static class Encryption
    {
        internal static string EncryptString(string plainText, string key)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] result;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                            {
                                streamWriter.Write(plainText);
                            }

                            result = memoryStream.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(result);
            }
            catch
            {
                throw;
            }
        }

        internal static string DecryptString(string cipherText, string key)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
