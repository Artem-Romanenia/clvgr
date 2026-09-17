using System.Collections.ObjectModel;

namespace clvgr.Controls;

internal class SettingsDialog : Dialog
{
    private readonly SettingsManager _settingsManager;
    private readonly DropDownList _themeField;
    private readonly NumericUpDown _clipboardPopupDurationSecondsField;
    private readonly Button _cancelBtn;

    public SettingsDialog(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;

        Width = Dim.Percent(70);
        Height = Dim.Auto();

        int fieldCounter = 0;

        var settings = UserSettings.Defaults;

        var label = CreateLabel("Theme", ++fieldCounter);
        Add(label, _themeField = new DropDownList()
        {
            Source = new ListWrapper<string>([with(ThemeManager.GetThemeNames())]),
            Value = settings.Theme,
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),

        });

        label = CreateLabel("Clipboard Popup Duration (seconds)", ++fieldCounter);
        Add(label, _clipboardPopupDurationSecondsField = new NumericUpDown()
        {
            Value = settings.ClipboardPopupDurationSeconds,
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        _themeField.Activated += OnThemeChanged;

        _cancelBtn = new() { Text = "_Cancel" };
        Button submitBtn = new() { Text = "_Apply and Close" };

        AddButton(_cancelBtn);
        AddButton(submitBtn);
    }

    protected override bool OnAccepting(CommandEventArgs args)
    {
        View? sourceView = null;
        args.Context?.Source?.TryGetTarget(out sourceView);

        if (sourceView == _cancelBtn)
        {
            this.PresentApp.RequestStop();

            return true;
        }

        return args.Context?.Source?.TryGetTarget(out sourceView) is not true || !Buttons.Contains(sourceView as Button);
    }

    protected override void OnAccepted(ICommandContext? ctx)
    {
        base.OnAccepted(ctx);

        _settingsManager.Apply(UserSettings.Defaults with
        {
            Theme = _themeField.Text,
            ClipboardPopupDurationSeconds = _clipboardPopupDurationSecondsField.Value,
        });
    }

    private static Label CreateLabel(string text, int fieldNumber) => new() { Text = $"{text}:".PadLeft(25), X = 2, Y = fieldNumber * 2 };

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (sender is not DropDownList l || l.Value is not string val) return;

        _settingsManager.SwitchTheme(val);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _themeField.Activated -= OnThemeChanged;

            _settingsManager.SwitchTheme(UserSettings.Defaults.Theme);
        }

        base.Dispose(disposing);
    }
}
