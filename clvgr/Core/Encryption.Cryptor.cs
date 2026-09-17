using System.Security.Cryptography;

namespace clvgr.Core;

internal partial class Encryption
{

    private class Cryptor(IProtectedMasterKey encryptedMasterKey) : ICryptor
    {
        public byte[] Encrypt(Span<byte> plaintext, bool keepPlaintext)
        {
            byte[] result = new byte[Constants.NonceLength + Constants.TagLength + plaintext.Length];

            Span<byte> nonce = result.AsSpan(0, Constants.NonceLength);
            Span<byte> tag = result.AsSpan(Constants.NonceLength, Constants.TagLength);
            Span<byte> ciphertext = result.AsSpan(Constants.NonceLength + Constants.TagLength);

            using SecureBuffer masterKey = new(Constants.SymmetricKeyLength);

            try
            {
                encryptedMasterKey.Fill(destination: masterKey);

                RandomNumberGenerator.Fill(nonce);

                using var aes = new AesGcm(masterKey, tagSizeInBytes: Constants.TagLength);
                aes.Encrypt(nonce, plaintext, ciphertext, tag);

                return result;
            }
            finally
            {
                if (!keepPlaintext) CryptographicOperations.ZeroMemory(plaintext);
            }
        }

        public void Decrypt(ReadOnlySpan<byte> encryptedData, Action<ReadOnlySpan<byte>> op)
        {
            if (encryptedData.Length < Constants.NonceLength + Constants.TagLength) throw new ArgumentException("Encrypted payload is malformed or truncated.");

            var nonce = encryptedData.Slice(0, Constants.NonceLength);
            var tag = encryptedData.Slice(Constants.NonceLength, Constants.TagLength);
            var ciphertext = encryptedData.Slice(Constants.NonceLength + Constants.TagLength);

            using SecureBuffer plaintext = new(ciphertext.Length);
            using SecureBuffer masterKey = new(Constants.SymmetricKeyLength);

            encryptedMasterKey.Fill(destination: masterKey);

            using var aes = new AesGcm(masterKey, tagSizeInBytes: Constants.TagLength);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            op(plaintext);
        }
    }
}
