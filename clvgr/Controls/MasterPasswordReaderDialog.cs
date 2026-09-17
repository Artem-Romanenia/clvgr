namespace clvgr.Controls;

internal class MasterPasswordReaderDialog : Dialog
{
    private readonly MasterSecretField _secretField;
    private readonly Action<ReadOnlySpan<byte>> _op;

    public MasterPasswordReaderDialog(string path, Action<ReadOnlySpan<byte>> op)
    {
        Height = Dim.Auto();
        Width = Dim.Auto(DimAutoStyle.Auto, Dim.Percent(90), 60);

        var pathLabel = new Label() { Text = $"Secrets file:" };
        var pathValueLable = new EllipsedLabel(path, ellipseLeft: true) { X = Pos.Right(pathLabel) + 1, Y = Pos.Top(pathLabel), Width = Dim.Fill() };
        var label = new Label() { Text = "Master Password", Width = Dim.Fill(), Y = Pos.Bottom(pathLabel) + 1 };
        _secretField = new MasterSecretField() { Y = Pos.Bottom(label), Width = Dim.Fill() };

        Add(pathLabel, pathValueLable, label, _secretField);
        _secretField.SetFocus();

        AddButton(new Button() { Text = "Submit" });

        _op = op;
    }

    public override void BeginInit()
    {
        Padding.Thickness = new Thickness(5);
        base.BeginInit();
    }

    protected override bool OnAccepting(CommandEventArgs args)
    {
        if (_secretField.SecretBytes.Length is 0)
        {
            MessageBox.ErrorQuery(this.PresentApp, "Validation Error", $"Nope.");
            return true;
        }

        return base.OnAccepting(args);
    }

    protected override void OnAccepted(ICommandContext? ctx)
    {
        _op(_secretField.SecretBytes);

        base.OnAccepted(ctx);
    }
}
