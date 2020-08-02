using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PacotePenseCre.Scripts.Utility
{
    public class EncryptionUtility
    {
        #region private variables

        private const int KeySize = 256;
        private const int BlockSize = KeySize; // keysize and blocksize need to be the same 
        private const int DerivationIterations = 1000;

        #endregion

        #region private methods

        private static byte[] GenerateRandom256Bits()
        {
            const int byteSize = KeySize / 8; // 1 byte = 8 bits
            var randomBytes = new byte[byteSize];
            var rngCsp = new RNGCryptoServiceProvider();
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
            return randomBytes;
        }

        #endregion


        #region public methods

        /// <summary>
        ///     Uses Rijanael algorithm 
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="passPhrase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string EncryptString(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = GenerateRandom256Bits();
            var ivStringBytes = GenerateRandom256Bits();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);

            var keyBytes = password.GetBytes(KeySize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = BlockSize;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();

                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                    var cipherTextBytes = saltStringBytes;
                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                    memoryStream.Close();
                    cryptoStream.Close();
                    return Convert.ToBase64String(cipherTextBytes);
                }
            }
        }

        /// <summary>
        ///     Uses Rijanael algorithm
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="passPhrase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static string DecryptString(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);

            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KeySize / 8).ToArray();

            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray();

            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes =
                cipherTextBytesWithSaltAndIv.Skip(KeySize / 8 * 2)
                    .Take(cipherTextBytesWithSaltAndIv.Length - KeySize / 8 * 2)
                    .ToArray();

            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);

            var keyBytes = password.GetBytes(KeySize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = BlockSize;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                using (var memoryStream = new MemoryStream(cipherTextBytes))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    var plainTextBytes = new byte[cipherTextBytes.Length];
                    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                    memoryStream.Close();
                    cryptoStream.Close();
                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                }
            }
        }

        #endregion


    }
}
