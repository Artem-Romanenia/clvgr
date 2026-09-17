using System.Runtime.Versioning;

namespace clvgr.Core;

internal partial class Encryption
{
    [SupportedOSPlatform("linux")]
    private class CryptorFactoryLinux : ICryptorFactory
    {
        public ICryptor Create(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> salt)
        {
            return new Cryptor(new ProtectedMasterKeyLinuxImpl(plaintext, salt));
        }
    }
}
