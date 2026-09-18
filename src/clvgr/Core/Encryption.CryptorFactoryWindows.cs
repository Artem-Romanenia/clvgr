using System.Runtime.Versioning;

namespace clvgr.Core;

internal partial class Encryption
{
    [SupportedOSPlatform("windows")]
    private class CryptorFactoryWindows : ICryptorFactory
    {
        public ICryptor Create(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> salt)
            => new Cryptor(new ProtectedMasterKeyWindowsImpl(plaintext, salt));
    }
}
