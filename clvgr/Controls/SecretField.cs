using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Terminal.Gui.Drivers;
using static clvgr.Core.Encryption;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace clvgr.Controls;

internal abstract class SecretFieldBase : View
{
    private static readonly UnicodeCategory[] _ignoreCategories = [UnicodeCategory.Control, UnicodeCategory.Format, UnicodeCategory.Surrogate, UnicodeCategory.OtherNotAssigned];

    private bool _lastHasFocus = false;

    protected readonly Stack<int> _charByteLengths = new();
    protected int _pos = 0;
    protected SecureMemory _secretBytes = new(1024);

    public SecretFieldBase()
    {
        Height = 1;
        CanFocus = true;

        SetScheme(new Scheme() { Normal = new Attribute(Color.White, Color.Black) });

        AddCommand(Command.Paste, ctx => throw new InvalidOperationException($"Bracketed paste should be disabled for '{GetType().Name}'"));
    }

    protected abstract string GetMaskedString();
    protected virtual void OnBackspace() { }
    protected virtual void OnGotFocus() { }
    protected virtual void OnLostFocus() { }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (!_lastHasFocus && HasFocus)
        {
            OnGotFocus();
            Console.Write(EscSeqUtils.CSI_DisableBracketedPaste);
        }
        else if (_lastHasFocus && !HasFocus)
        {
            OnLostFocus();
            Console.Write(EscSeqUtils.CSI_EnableBracketedPaste);
        }

        _lastHasFocus = HasFocus;

        Move(0, 0);
        AddStr(GetMaskedString());
        return base.OnDrawingContent(context);
    }

    protected override bool OnKeyDown(Key key)
    {
        var secretBytes = _secretBytes.AsSpan();
        var rune = key.AsRune;

        if (!_ignoreCategories.Contains(Rune.GetUnicodeCategory(rune)))
        {
            Span<byte> runeBytes = stackalloc byte[4];

            int runeLength = key.AsRune.EncodeToUtf8(runeBytes);

            _charByteLengths.Push(runeLength);

            for (int i = 0; i < runeLength; i++)
            {
                secretBytes[_pos++] = runeBytes[i];
            }

            SetNeedsDraw();
            return true;
        }
        else if (key.KeyCode == KeyCode.Backspace)
        {
            if (_charByteLengths.TryPop(out int lastCharLength))
            {
                _pos -= lastCharLength;
                CryptographicOperations.ZeroMemory(_secretBytes.AsSpan(_pos, lastCharLength));
            }

            OnBackspace();

            SetNeedsDraw();
            return true;
        }

        return base.OnKeyDown(key);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _secretBytes.Dispose();
            Console.Write(EscSeqUtils.CSI_EnableBracketedPaste);
        }

        base.Dispose(disposing);
    }
}

internal class MasterSecretField : SecretFieldBase
{
    public Span<byte> SecretBytes => _secretBytes.AsSpan(0, _pos);

    protected override string GetMaskedString() => string.Empty.PadRight(_charByteLengths.Count, '*');
}

internal class SecretField(ICryptor cryptor, byte[]? encryptedSecretBytes = null) : SecretFieldBase
{
    private byte[]? _secretBytesEncrypted = encryptedSecretBytes;

    public byte[] EncryptedSecretBytes => _secretBytesEncrypted ?? (_pos > 0 ? cryptor.Encrypt(_secretBytes.AsSpan(0, _pos)) : []);

    protected override string GetMaskedString() => _secretBytesEncrypted is { Length: > 0 } && _pos == 0 ? "[encrypted]" : string.Empty.PadRight(_charByteLengths.Count, '*');

    protected override void OnBackspace()
    {
        if (_secretBytesEncrypted is { } && _pos == 0)
        {
            _secretBytesEncrypted = null;
        }
    }

    protected override void OnGotFocus()
    {
        if (_secretBytesEncrypted is { Length: > 0 })
        {
            cryptor.Decrypt(_secretBytesEncrypted, sec => sec.CopyTo(_secretBytes.AsSpan()));
        }
    }

    protected override void OnLostFocus()
    {
        if (_pos > 0)
        {
            _secretBytesEncrypted = cryptor.Encrypt(_secretBytes.AsSpan(0, _pos));
            _secretBytes.Clear();
       }
    }
}
