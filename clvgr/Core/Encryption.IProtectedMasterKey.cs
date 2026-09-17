namespace clvgr.Core;

internal partial class Encryption
{
    internal interface IProtectedMasterKey
    {
        void Fill(Span<byte> destination);
    }
}