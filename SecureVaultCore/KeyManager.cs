using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureVaultCore
{
    /// <summary>
    /// Utility class for managing AES-256 encryption keys.
    /// </summary>
    public class KeyManager
    {
        /// <summary>
        /// Generates a new AES-256 key (32 bytes = 256 bits).
        /// </summary>
        /// <returns>A Base64-encoded AES-256 key.</returns>
        public static string GenerateAes256Key()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        /// <summary>
        /// Generates a new AES-256 IV (Initialization Vector - 16 bytes for AES).
        /// </summary>
        /// <returns>A Base64-encoded IV.</returns>
        public static string GenerateAes256IV()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateIV();
                return Convert.ToBase64String(aes.IV);
            }
        }

        /// <summary>
        /// Encrypts plaintext using AES-256 CBC mode.
        /// </summary>
        /// <param name="plaintext">The text to encrypt.</param>
        /// <param name="keyBase64">The Base64-encoded AES key.</param>
        /// <param name="ivBase64">The Base64-encoded IV.</param>
        /// <returns>A Base64-encoded ciphertext.</returns>
        public static string EncryptAes256(string plaintext, string keyBase64, string ivBase64)
        {
            byte[] key = Convert.FromBase64String(keyBase64);
            byte[] iv = Convert.FromBase64String(ivBase64);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                    return Convert.ToBase64String(ciphertext);
                }
            }
        }

        /// <summary>
        /// Decrypts ciphertext using AES-256 CBC mode.
        /// </summary>
        /// <param name="ciphertextBase64">The Base64-encoded ciphertext.</param>
        /// <param name="keyBase64">The Base64-encoded AES key.</param>
        /// <param name="ivBase64">The Base64-encoded IV.</param>
        /// <returns>The decrypted plaintext.</returns>
        public static string DecryptAes256(string ciphertextBase64, string keyBase64, string ivBase64)
        {
            byte[] key = Convert.FromBase64String(keyBase64);
            byte[] iv = Convert.FromBase64String(ivBase64);
            byte[] ciphertext = Convert.FromBase64String(ciphertextBase64);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] plaintextBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
                    return Encoding.UTF8.GetString(plaintextBytes);
                }
            }
        }

        /// <summary>
        /// Validates if a Base64 string is a valid AES-256 key (32 bytes).
        /// </summary>
        public static bool IsValidAes256Key(string keyBase64)
        {
            try
            {
                byte[] key = Convert.FromBase64String(keyBase64);
                return key.Length == 32; // 256 bits = 32 bytes
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a Base64 string is a valid IV (16 bytes for AES).
        /// </summary>
        public static bool IsValidIV(string ivBase64)
        {
            try
            {
                byte[] iv = Convert.FromBase64String(ivBase64);
                return iv.Length == 16; // 128 bits = 16 bytes
            }
            catch
            {
                return false;
            }
        }
    }
}
