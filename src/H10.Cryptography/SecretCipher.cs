using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace H10.Cryptography
{
    public class SecretCipher
    {
        private readonly IConfiguration _configuration;
        private readonly string _keyVaultUri;
        private readonly string _keyVaultSecretName;

        private KeyVaultSecret _keyVaultSecret;
        
        public SecretCipher(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _keyVaultUri = configuration["Azure:KeyVault_Uri"];
            _keyVaultSecretName = configuration["Azure:KeyVault_SecretName"];

            this.DefaultInit();
        }

        public SecretCipher(IConfiguration configuration, string keyVaultUri, string keyVaultSecretName)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _keyVaultUri = keyVaultUri ?? throw new ArgumentNullException(nameof(keyVaultUri));
            _keyVaultSecretName = keyVaultSecretName ?? throw new ArgumentNullException(nameof(keyVaultSecretName));
            
            this.DefaultInit();
        }

        public SecretCipher(IConfiguration configuration, string keyVaultSecretName)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _keyVaultUri = configuration["Azure:KeyVault_Uri"];
            _keyVaultSecretName = keyVaultSecretName ?? throw new ArgumentNullException(nameof(keyVaultSecretName));
            
            this.DefaultInit();
        }

        protected void DefaultInit()
        {
            var secretClient = new SecretClient(new Uri(_keyVaultUri), new DefaultAzureCredential());
            _keyVaultSecret = secretClient.GetSecret(_keyVaultSecretName);
        }
        
        public string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));

            //Key should be of size 32bytes & iv should be of size 16bytes
            byte[] key = Encoding.ASCII.GetBytes(_keyVaultSecret.Value.PadLeft(32));
            byte[] encrypted;
            byte[] iv;

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;

                aesAlg.Key = key;
                aesAlg.GenerateIV();

                iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Prepend IV to data
                            msEncrypt.Write(iv, 0, iv.Length);
                            swEncrypt.Write(value);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Marshal for http transport
            string base64EncodedString = Convert.ToBase64String(encrypted);
            byte[] base64Utfbytes = System.Text.Encoding.UTF8.GetBytes(base64EncodedString);

            return Convert.ToBase64String(base64Utfbytes);
        }

        public string Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            
            byte[] base64Utfbytes = Convert.FromBase64String(value);
            string base64StringValue = Encoding.UTF8.GetString(base64Utfbytes);

            //Key should be of size 32bytes & iv should be of size 16bytes
            byte[] key = Encoding.ASCII.GetBytes(_keyVaultSecret.Value.PadLeft(32));
            byte[] cipherText = Convert.FromBase64String(base64StringValue);
            byte[] iv = new byte[16];

            string plaintext = null;

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {                                
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    msDecrypt.Read(iv, 0, 16);

                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;

                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}