using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace clvgr.Core;

internal unsafe ref struct SecureBuffer
{
    private byte* _ptr;
    private readonly int _length;

    public SecureBuffer(int length)
    {
        _ptr = (byte*)NativeMemory.AllocZeroed((nuint)length);
        _length = length;
    }

    public void Dispose()
    {
        if (_ptr != null)
        {
            CryptographicOperations.ZeroMemory(this);
            NativeMemory.Free(_ptr);
            _ptr = null;
        }
    }

    public static implicit operator Span<byte>(SecureBuffer buff) => buff._ptr is not null
        ? new Span<byte>(buff._ptr, buff._length)
        : throw new ObjectDisposedException(nameof(SecureBuffer));
}

internal unsafe class SecureMemory
{
    private byte* _ptr;
    private readonly int _length;

    public SecureMemory(int length)
    {
        _ptr = (byte*)NativeMemory.AllocZeroed((nuint)length);
        _length = length;
    }

    public Span<byte> AsSpan(int from = 0, int? length = null) => _ptr is not null
        ? new Span<byte>(_ptr + from, length ?? _length)
        : throw new ObjectDisposedException(nameof(SecureMemory));

    public void Clear()
    {
        CryptographicOperations.ZeroMemory(AsSpan());
    }

    public void Dispose()
    {
        if (_ptr != null)
        {
            CryptographicOperations.ZeroMemory(AsSpan());
            NativeMemory.Free(_ptr);
            _ptr = null;
        }
    }
}
