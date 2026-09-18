using clvgr.Core.Exceptions;
using System.Runtime.InteropServices;
using System.Text;

namespace clvgr.Core;

internal partial class Clipboard
{
    private class ClipboardWindows : IClipboardAccessor
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EmptyClipboard();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        private const uint GMEM_MOVEABLE = 0x0002;

        public unsafe void PutToClipboard(ReadOnlySpan<byte> utf8Bytes)
        {
            if (!OpenClipboard(IntPtr.Zero)) throw new ClipboardException("Failed to open clipboard.");

            IntPtr hGlobal = IntPtr.Zero;

            try
            {
                if (!EmptyClipboard())
                {
                    CloseClipboard();
                    throw new ClipboardException("Failed to empty a clipboard");
                }

                int charCount = Encoding.UTF8.GetCharCount(utf8Bytes);

                UIntPtr totalBytes = (UIntPtr)((charCount + 1) * sizeof(char));
                hGlobal = GlobalAlloc(GMEM_MOVEABLE, totalBytes);
                if (hGlobal == IntPtr.Zero)
                {
                    CloseClipboard();
                    throw new ClipboardException("Failed to allocate memory for a clipboard.");
                }

                IntPtr pTarget = GlobalLock(hGlobal);
                if (pTarget == IntPtr.Zero)
                {
                    GlobalFree(hGlobal);
                    CloseClipboard();
                    throw new ClipboardException("Failed to lock clipboard memory.");
                }

                try
                {
                    Span<char> targetSpan = new Span<char>((void*)pTarget, charCount);

                    Encoding.UTF8.GetChars(utf8Bytes, targetSpan);

                    ((char*)pTarget)[charCount] = '\0';
                }
                finally
                {
                    GlobalUnlock(hGlobal);
                }

                if (SetClipboardData(13, hGlobal) != IntPtr.Zero)
                {
                    hGlobal = IntPtr.Zero;
                }
                else
                {
                    throw new ClipboardException("Failed to set clipboard data.");
                }
            }
            finally
            {
                if (hGlobal != IntPtr.Zero) GlobalFree(hGlobal);
                CloseClipboard();
            }
        }
    }
}
