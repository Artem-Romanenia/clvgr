namespace clvgr.Controls;

internal class MainMenu : MenuBar
{
    public MainMenu(AppManager appManager)
    {
        Add(new MenuBarItem("🗎 _File", [
            new MenuItem(commandText: "Add/Open _Secrets file", action: appManager.SelectSecretsFile),
            new MenuItem(commandText: "_Persist secrets file", action: () => { }),
        ]));
        Add(new MenuItem(commandText: "🔧 S_ettings", action: appManager.ShowPreferencesDialog));
        Add(new MenuItem(commandText: "🛈 _About", action: appManager.ShowAboutDialog));
    }
}
