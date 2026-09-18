using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace clvgr.Core;

internal partial class Encryption
{
    [SupportedOSPlatform("linux")]
    private partial class ProtectedMasterKeyLinuxImpl : IProtectedMasterKey, IDisposable
    {
        private const int PR_SET_DUMPABLE = 4;

        [LibraryImport("libc", SetLastError = true)]
        private static partial int prctl(int option, ulong arg2, ulong arg3, ulong arg4, ulong arg5);

        [LibraryImport("libc", SetLastError = true)]
        private static partial int mlock(byte* addr, nuint len);

        [LibraryImport("libc", SetLastError = true)]
        private static partial int munlock(byte* addr, nuint len);

        private readonly SecureMemory _masterKeyStorage;

        public ProtectedMasterKeyLinuxImpl(ReadOnlySpan<byte> masterPasswordPlaintext, ReadOnlySpan<byte> salt)
        {
            _masterKeyStorage = new SecureMemory(Constants.SymmetricKeyLength);

            LockMemoryToRam();
            DisableProcessDumping();

            Span<byte> destinationKey = _masterKeyStorage.AsSpan();
            Rfc2898DeriveBytes.Pbkdf2(masterPasswordPlaintext, salt, destinationKey, 600_000, HashAlgorithmName.SHA256);
        }

        public void Fill(Span<byte> destination) => _masterKeyStorage.AsSpan().CopyTo(destination);

        private unsafe void LockMemoryToRam()
        {
            Span<byte> span = _masterKeyStorage.AsSpan();
            fixed (byte* ptr = span)
            {
                if (mlock(ptr, (nuint)span.Length) != 0)
                {
                    int error = Marshal.GetLastPInvokeError();
                    throw new InvalidOperationException($"mlock failed with error code: {error}. Check memory lock limits.");
                }
            }
        }

        private static void DisableProcessDumping()
        {
            if (prctl(PR_SET_DUMPABLE, 0, 0, 0, 0) != 0)
            {
                int error = Marshal.GetLastPInvokeError();
                throw new InvalidOperationException($"prctl(PR_SET_DUMPABLE) failed with error code: {error}");
            }
        }

        public unsafe void Dispose()
        {
            Span<byte> span = _masterKeyStorage.AsSpan();
            fixed (byte* ptr = span)
            {
                munlock(ptr, (nuint)span.Length);
            }

            _masterKeyStorage.Dispose();
        }
    }
}
