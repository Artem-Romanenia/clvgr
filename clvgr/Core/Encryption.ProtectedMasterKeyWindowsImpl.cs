using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace clvgr.Core;

internal partial class Encryption
{
    [SupportedOSPlatform("windows")]
    private class ProtectedMasterKeyWindowsImpl : IProtectedMasterKey, IDisposable
    {
        private byte[] _encryptedMasterKey;

        private readonly SecureMemory _sessionEntropy;
        public ProtectedMasterKeyWindowsImpl(ReadOnlySpan<byte> masterPasswordPlaintext, ReadOnlySpan<byte> salt)
        {
            _sessionEntropy = new SecureMemory(Constants.SessionEntropyLength);
            RandomNumberGenerator.Fill(_sessionEntropy.AsSpan());

            using SecureBuffer masterKey = new SecureBuffer(Constants.SymmetricKeyLength);

            Rfc2898DeriveBytes.Pbkdf2(masterPasswordPlaintext, salt, masterKey, 600_000, HashAlgorithmName.SHA256);

            _encryptedMasterKey = ProtectedData.Protect(masterKey, DataProtectionScope.CurrentUser, optionalEntropy: _sessionEntropy.AsSpan());
        }

        public void Fill(Span<byte> destination)
        {
            ProtectedData.Unprotect(_encryptedMasterKey, DataProtectionScope.CurrentUser, destination: destination, optionalEntropy: _sessionEntropy.AsSpan());
        }

        public void Dispose()
        {
            _sessionEntropy.Dispose();
        }
    }
}
