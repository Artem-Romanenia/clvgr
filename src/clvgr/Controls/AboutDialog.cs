using Logging = clvgr.Core.Logging;

namespace clvgr.Controls;

internal class AboutDialog : Dialog
{
    private readonly Logo _logo;

    public AboutDialog(SecretsFileManager? secretsFileManager)
    {
        Width = Dim.Percent(70);
        Height = Dim.Auto();

        _logo = new Logo() { X = Pos.Center() };

        Add(_logo);

        int fieldCounter = 0;

        var label = CreateLabel("Home", ++fieldCounter);
        Add(label, new EllipsedLabel("https://github.com/Artem-Romanenia/clvgr")
        {
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        label = CreateLabel("Settings file", ++fieldCounter);
        Add(label, new EllipsedLabel(SettingsManager.SettingsFilePath)
        {
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        label = CreateLabel("Logs file", ++fieldCounter);
        Add(label, new EllipsedLabel(Logging.LogsFilePath)
        {
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        label = CreateLabel("Secrets file", ++fieldCounter);
        Add(label, new EllipsedLabel(secretsFileManager?.SecretsFileName ?? string.Empty)
        {
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });
    }

    private Label CreateLabel(string text, int fieldNumber) => new() { Text = $"{text}:".PadLeft(25), X = 2, Y = _logo.HeightInt + 1 + fieldNumber * 2 };
}
