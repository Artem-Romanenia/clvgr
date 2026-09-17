namespace clvgr.Controls;

internal class EllipsedLabel : View
{
    private readonly string _text;
    private readonly bool _ellipseLeft;
    private int _pos;

    public EllipsedLabel(string text, bool ellipseLeft = false)
    {
        CanFocus = true;
        Height = 1;

        _text = text;
        _ellipseLeft = ellipseLeft;
    }

    protected override void OnHasFocusChanged(bool newHasFocus, View? previousFocusedView, View? focusedView)
    {
        int width = GetContentWidth();
        if (newHasFocus && _text.Length > width)
        {
            this.PresentApp.AddTimeout(TimeSpan.FromMilliseconds(100), () =>
            {
                _pos += 1;
                SetNeedsDraw();
                return HasFocus && _pos < _text.Length - width;
            });
        }
        else if (!newHasFocus)
        {
            _pos = 0;
        }

        base.OnHasFocusChanged(newHasFocus, previousFocusedView, focusedView);
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        Move(0,0);

        var cs = GetScheme();
        int width = GetContentWidth();

        SetAttribute(HasFocus ? cs.Focus : cs.Normal);

        string sp = (_text.Length > width, HasFocus, _ellipseLeft) switch
        {
            (false, _, _) => _text,
            (true, false, false) => string.Concat(_text.AsSpan(0, width - 3), "..."),
            (true, false, true) => string.Concat("...", _text.AsSpan(_text.Length - width + 3)),
            (true, true, _) => _text.Substring(_pos, width)
        };
        AddStr(sp);

        return true;
    }
}
