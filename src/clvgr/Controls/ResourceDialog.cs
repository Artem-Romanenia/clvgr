using Google.Protobuf;
using static clvgr.Core.Encryption;

namespace clvgr.Controls;

internal class ResourceDialog : Dialog<Resource>
{
    private readonly TextField _resourceNameField;
    private readonly TextField _shortNameField;
    private readonly TextField _urlField;
    private readonly TextField _tagsField;
    private readonly SecretField _secretField;
    private Dictionary<int, (TextField name, InputField value, CheckBox secureCh)>? _addtitionalFields;
    private readonly ICryptor _cryptor;

    public ResourceDialog(ICryptor cryptor, Resource? resource)
    {
        Title = resource is { } ? "Edit resource" : "Create resource";
        int fieldCounter = 0;

        var label = CreateLabel("Resource name", ++fieldCounter);
        Add(label, _resourceNameField = new TextField()
        {
            Value = resource?.ResourceName,
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        label = CreateLabel("Short name", ++fieldCounter);
        Add(label, _shortNameField = new TextField()
        {
            Value = resource?.ShortName,
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        label = CreateLabel("Url", ++fieldCounter);
        Add(label, _urlField = new TextField()
        {
            Value = resource?.Url,
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        label = CreateLabel("Tags", ++fieldCounter);
        Add(label, _tagsField = new TextField()
        {
            Value = resource?.TagString,
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        label = CreateLabel("Secret", ++fieldCounter);
        Add(label, _secretField = new SecretField(cryptor, resource?.Secret.ToArray())
        {
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill(2),
        });

        Button addBtn = new() { Text = "_Additional field" };
        addBtn.HotKeyBindings.Clear();
        addBtn.Accepting += (s, e) =>
        {
            AddAdditionalField(null);
            e.Handled = true;
        };

        AddCommand(Command.Insert, AddAdditionalField);
        KeyBindings.Add(Key.A.WithAlt, [Command.Insert]);

        Button submitBtn = new() { Text = resource is null ? "Create" : "Update" };

        AddButton(addBtn);
        AddButton(submitBtn);
        _cryptor = cryptor;
    }

    private bool? AddAdditionalField(ICommandContext? ctx)
    {
        _addtitionalFields ??= [];

        int idx = _addtitionalFields.Count;

        TextField label = new()
        {
            X = 2,
            Y = 12 + _addtitionalFields.Count * 2,
            Width = 25
        };
        TextField value = new()
        {
            X = Pos.Right(label) + 1,
            Y = Pos.Top(label),
            Width = Dim.Fill() - 6,
        };
        CheckBox secureCh = new()
        {
            X = Pos.AnchorEnd(5),
            Y = Pos.Top(label),
            Width = 5,
            Text = "🔐",
            Data = idx
        };
        secureCh.ValueChanged += OnAdditionalFIeldTypeChanged;

        _addtitionalFields.Add(idx, (label, value, secureCh));

        Add(label, value, secureCh);
        label.SetFocus();

        return true;
    }

    protected override bool OnAccepting(CommandEventArgs args)
    {
        if (_resourceNameField.Text is not { Length: > 0 })
        {
            MessageBox.ErrorQuery(this.PresentApp, "Validation Error", $"'Resource name' is required.");
            return true;
        }

        if (_secretField.EncryptedSecretBytes is not { Length: > 0 })
        {
            MessageBox.ErrorQuery(this.PresentApp, "Validation Error", $"'Secret' is required.");
            return true;
        }

        Result = new Resource
        {
            ResourceName = _resourceNameField.Text,
            Secret = ByteString.CopyFrom(_secretField.EncryptedSecretBytes),
            ShortName = _shortNameField.Text,
            Url = _urlField.Text,
        };

        Result.Tags.Clear();
        Result.Tags.AddRange(_tagsField.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.TrimStart('#')));

        Result.Fields.Clear();
        Result.Fields.AddRange(_addtitionalFields?.Values.Select(f => f.value switch
        {
            SecretField sf => new AdditionalField() { Name = f.name.Text, SecretValue = ByteString.CopyFrom(sf.EncryptedSecretBytes) },
            TextField tf => new AdditionalField() { Name = f.name.Text, OpenValue = tf.Text }
        }) ?? []);

        return base.OnAccepting(args);
    }

    private static Label CreateLabel(string text, int fieldNumber) => new() { Text = $"{text}:".PadLeft(25), X = 2, Y = fieldNumber * 2 };

    private void OnAdditionalFIeldTypeChanged(object? sender, ValueChangedEventArgs<CheckState> e)
    {
        if (sender is CheckBox ch && ch.Data is int idx)
        {
            var (lbl, val, sec) = _addtitionalFields![idx];
            var v = val.AsView();

            InputField newVal = e.NewValue == CheckState.Checked
                ? new SecretField(_cryptor) { X = v.X, Y = v.Y, Width = v.Width }
                : new TextField() { X = v.X, Y = v.Y, Width = v.Width };

            _addtitionalFields[idx] = (lbl, newVal, sec);

            int guiIdx = SubViews.IndexOf(v);
            Remove(v);
            AddAt(guiIdx, newVal.AsView());
            v.Dispose();
        }
    }

    private union InputField(SecretField, TextField)
    {
        public readonly View AsView() => this switch
        {
            SecretField sf => sf,
            TextField tf => tf
        };
    }
}
