namespace clvgr.Controls;

internal class AutoclosePopup : Dialog
{
    private bool _timeoutSet;

    public AutoclosePopup(string message) => Add(new Label() { Text = message, Width = Dim.Auto() });

    protected override void OnDrawComplete(DrawContext? context)
    {
        base.OnDrawComplete(context);

        if (_timeoutSet)
        {
            return;
        }

        this.PresentApp.AddTimeout(TimeSpan.FromSeconds(UserSettings.Defaults.ClipboardPopupDurationSeconds), () =>
        {
            RequestStop();
            return false;
        });
        _timeoutSet = true;
    }
}
