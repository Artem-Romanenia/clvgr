namespace clvgr.Controls;

internal class MainFooter : StatusBar
{
    private static Shortcut[] _mainContentShortcuts = [
        new Shortcut(Key.InsertChar, "Add new resource", null),
        new Shortcut(Key.Space, "Copy secret to clipboard", null),
        new Shortcut(Key.Enter, "Edit", null),
        new Shortcut(Key.DeleteChar, "Delete", null),
    ];

    private static Shortcut[] _resourceDialogShortcuts = [
        new Shortcut(Key.A.WithAlt, "Add additional field", null),
        new Shortcut(Key.Esc, "Cancel", null),
        new Shortcut(Key.Enter, "Submit", null),
    ];

    private static Shortcut[] _aboutDialogShortcuts = [
        new Shortcut(Key.Esc, "Close", null),
    ];

    private static Shortcut[] settingsDialogShortcuts = [
        new Shortcut(Key.A.WithAlt, "Apply and Close", null),
        new Shortcut(Key.C.WithAlt, "Cancel", null),
        new Shortcut(Key.Esc, "Cancel", null),
    ];

    private static Shortcut[] _settingsDialogDropdownShortcuts = [
        new Shortcut(Key.Space, "Open Dropdown", null),
        .. settingsDialogShortcuts,
    ];

    private static Shortcut[] _settingsDialogNumericShortcuts = [
        new Shortcut(Key.CursorUp, "+1", null),
        new Shortcut(Key.CursorDown, "-1", null),
        .. settingsDialogShortcuts,
    ];

    private static Shortcut[] _listViewShortcuts = [
        new Shortcut(Key.CursorUp, "Up", null),
        new Shortcut(Key.CursorDown, "Down", null),
        new Shortcut(Key.Space, "Select", null),
        new Shortcut(Key.Esc, "Close", null),
    ];

    private static Shortcut[] _tableViewShortcuts = [
        new Shortcut(Key.CursorUp, "Up", null),
        new Shortcut(Key.CursorDown, "Down", null),
        new Shortcut(Key.Backspace, "One folder back", null),
        new Shortcut(Key.Enter, "Select", null),
        new Shortcut(Key.Esc, "Close", null),
    ];

    private static Shortcut[] _buttonShortcuts = [
        new Shortcut(Key.Space, "Press", null),
        new Shortcut(Key.Enter, "Press", null),
    ];

    public override void BeginInit()
    {
        base.BeginInit();

        App?.Navigation?.FocusedChanged += OnFocusChahged;
    }

    private void OnFocusChahged(object? sender, EventArgs e)
    {
        RemoveAll();

        var focused = App?.Navigation?.GetFocused();

        View[] shortcuts = focused switch
        {
            MainContent c when c.SubViews.SingleOrDefault(v => v is ResourceList) is { } => _mainContentShortcuts,
            ResourceListItem => _mainContentShortcuts,
            { SuperView: ResourceDialog } => _resourceDialogShortcuts,
            { SuperView: AboutDialog } => _aboutDialogShortcuts,
            DropDownList and { SuperView: SettingsDialog } => _settingsDialogDropdownShortcuts,
            { SuperView: NumericUpDown and { SuperView: SettingsDialog } } => _settingsDialogNumericShortcuts,
            ListView => _listViewShortcuts,
            TableView => _tableViewShortcuts,
            Button => _buttonShortcuts,
            _ => [
#if DEBUG
                new Label() { Text = $"{focused?.GetType().Name ?? string.Empty}" }
#endif
            ]
        };

        Add(shortcuts);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            App?.Navigation?.FocusedChanged -= OnFocusChahged;
        }

        base.Dispose(disposing);
    }
}
