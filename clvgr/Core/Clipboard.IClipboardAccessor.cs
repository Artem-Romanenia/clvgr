namespace clvgr.Core;

internal partial class Clipboard
{
    internal interface IClipboardAccessor
    {
        void PutToClipboard(ReadOnlySpan<byte> encryptedData);
    }
}
