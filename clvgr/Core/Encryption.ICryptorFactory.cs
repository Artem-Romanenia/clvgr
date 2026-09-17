namespace clvgr.Core;

internal partial class Encryption
{
    internal interface ICryptorFactory
    {
        ICryptor Create(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> salt);
    }
}
