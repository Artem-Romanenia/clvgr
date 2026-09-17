namespace clvgr.Extensions;

internal static class Extensions
{
    extension(IApplication app)
    {
        public void ShowWarning(string title, string message) => MessageBox.Query(app, title, message, "Ok");

        public void ShowError(string title, string message) => MessageBox.ErrorQuery(app, title, message, "Ok");
    }

    extension(View view)
    {
        public IApplication PresentApp => view.App ?? throw new InvalidOperationException($"'{nameof(view.App)}' is expected to be not null here.");
    }
}
