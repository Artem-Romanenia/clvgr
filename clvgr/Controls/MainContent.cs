namespace clvgr.Controls;

internal class MainContent : View
{
    public MainContent()
    {
        CanFocus = true;
        TabStop = TabBehavior.TabGroup;
    }

    public override void BeginInit()
    {
        Margin.Thickness = new Thickness(0, 1, 0, 1);
        base.BeginInit();
    }
    public void Show(View view)
    {
        view.Width = Dim.Fill();
        view.Height = Dim.Fill();

        RemoveAll();
        Add(view);
    }
}
