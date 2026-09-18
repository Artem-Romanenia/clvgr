namespace clvgr.Core;

internal partial class Encryption
{
    internal interface ICryptor
    {
        byte[] Encrypt(Span<byte> plaintext, bool keepPlaintext = false);
        void Decrypt(ReadOnlySpan<byte> encryptedData, Action<ReadOnlySpan<byte>> op);
    }
}
