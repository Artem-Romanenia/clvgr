namespace clvgr.Controls;

internal class Logo : View
{
    private readonly string[] _logoStrings = [
        @"  _________                               ",
        @" / ________\ ___   __   __   ____   _____ ",
        @"/\ \_______//\  \ /\ \ /\ \ / __ \ /\  __\",
        @"\ \ \       \//\ \\ \ \_/ //\ \L\ \\ \ \_/ ",
        @" \ \ \_______ \ \ \\ \___/ \ \____ \\ \_\ ",
        @"  \ \________\ \_\ \\/__/   \/___L\ \\/_/ ",
        @"   \/________/ /\____\        /\____/     ",
        @"               \/____/        \/___/      "
    ];

    public int HeightInt => _logoStrings.Length;

    public Logo()
    {
        Width = _logoStrings[0].Length + 2;
        Height = _logoStrings.Length + 2;
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        for (int i = 0; i < _logoStrings.Length; i++)
        {
            Move(0, i);
            this.PresentApp.Driver?.AddStr(_logoStrings[i]);
        }

        return base.OnDrawingContent(context);
    }
}
