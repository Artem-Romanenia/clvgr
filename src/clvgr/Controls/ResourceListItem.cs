namespace clvgr.Controls;

internal class ResourceListItem : View
{
    public ResourceListItem(Resource resource)
    {
        CanFocus = true;
        TabStop = TabBehavior.TabStop;

        Height = 1;
        Resource = resource;
    }

    public Resource Resource { get; }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (App is null || App.Driver is null) throw new ArgumentException("");

        Move(0, 0);

        var cs = GetScheme();

        int colWidth = GetContentWidth() / 3;

        SetAttribute(HasFocus ? cs.Focus : cs.CodeIdentifier);
        AddStr(FormatString($"{Resource.ResourceName}{(Resource.ShortName is { Length: > 0 } ? $" ({Resource.ShortName})" : string.Empty)}", colWidth));

        SetAttribute(HasFocus ? cs.Focus : cs.CodeConstant);
        AddStr(FormatString(Resource.Url ?? string.Empty, colWidth));

        SetAttribute(HasFocus ? cs.Focus : cs.CodeComment);
        AddStr(FormatString(string.Join(", ", Resource.Tags?.Select(t => $"#{t}") ?? []), colWidth));

        SetAttribute(cs.Normal);

        return true;
    }

    private static string FormatString(string s, int width)
    {
        if (s.Length > width)
        {
            s = string.Concat(s.AsSpan(0, width - 3), "...");
        }
        else if (s.Length < width)
        {
            s = s.PadRight(width);
        }

        return $" {s}";
    }
}
