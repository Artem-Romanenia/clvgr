using System.Security.Cryptography;

namespace clvgr.Core;

internal static class Constants
{
    public const int SymmetricKeyLength = 32;
    public const int SaltLength = 16;
    public const int SessionEntropyLength = 32;
    public static int NonceLength => AesGcm.NonceByteSizes.MaxSize;
    public static int TagLength => AesGcm.TagByteSizes.MaxSize;
}
