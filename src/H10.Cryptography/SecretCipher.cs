using System;

namespace H10.Cryptography
{
    public class SecretCipher
    {
        private SecretCipher()
        {
        }
        
        public string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));

            return string.Empty;
        }

        public string Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            
            return string.Empty;
        }
    }
}