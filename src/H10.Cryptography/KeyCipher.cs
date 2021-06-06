using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Configuration;

namespace H10.Cryptography
{
    public class KeyCipher
    {
        private readonly IConfiguration _configuration;
        private readonly string _keyVaultUri;
        private readonly string _keyVaultKeyName;

        private CryptographyClient _cryptoClient;

        public KeyCipher(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _keyVaultUri = configuration["Azure:KeyVault_Uri"];
            _keyVaultKeyName = configuration["Azure:KeyVault_KeyName"];

            this.DefaultInit();
        }

        public KeyCipher(IConfiguration configuration, string keyVaultUri, string keyVaultKeyName)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _keyVaultUri = keyVaultUri ?? throw new ArgumentNullException(nameof(keyVaultUri));
            _keyVaultKeyName = keyVaultKeyName ?? throw new ArgumentNullException(nameof(keyVaultKeyName));
            
            this.DefaultInit();
        }

        public KeyCipher(IConfiguration configuration, string keyVaultKeyName)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _keyVaultUri = configuration["Azure:KeyVault_Uri"];
            _keyVaultKeyName = keyVaultKeyName ?? throw new ArgumentNullException(nameof(keyVaultKeyName));
            
            this.DefaultInit();
        }

        protected void DefaultInit()
        {
            string tenantId = _configuration["Azure:Tenant_ID"];
            string clientId = _configuration["Azure:Client_ID"];
            string clientSecret = _configuration["Azure:Client_Secret"];

            var clientCredentials = new ClientSecretCredential(tenantId, clientId, clientSecret);
            
            var keyClient = new KeyClient(new Uri(_keyVaultUri), clientCredentials);
            keyClient.CreateRsaKey(new CreateRsaKeyOptions(_keyVaultKeyName));

            KeyVaultKey key = keyClient.GetKey(_keyVaultKeyName);
            _cryptoClient = new CryptographyClient(key.Id, new DefaultAzureCredential());
        }

        public string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));

            byte[] valueByteArray = Encoding.UTF8.GetBytes(value);
            EncryptResult encryptResult = _cryptoClient.Encrypt(EncryptionAlgorithm.RsaOaep, valueByteArray);

            return Convert.ToBase64String(encryptResult.Ciphertext);
        }

        public string Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));

            byte[] encryptedByteArray = Convert.FromBase64String(value);
            DecryptResult decryptResult =
                _cryptoClient.Decrypt(EncryptionAlgorithm.RsaOaep, encryptedByteArray);

            return Encoding.Default.GetString(decryptResult.Plaintext);
        }
    }
}