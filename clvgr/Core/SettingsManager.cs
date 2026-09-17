using System.Text.Json;

namespace clvgr.Core;

internal class SettingsManager
{
    private static readonly string _settingsFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "clvgr");

    public static string SettingsFilePath { get; } = Path.Combine(_settingsFileDir, "settings.json");

    private readonly TuiConfigurationBuilder? _configurationBuilder;


    public SettingsManager()
    {
        _configurationBuilder = new("clvgr")
        {
            RuntimeConfig = File.Exists(SettingsFilePath) ? File.ReadAllText(SettingsFilePath) : null
        };

        _configurationBuilder.BindAppSettings<UserSettings>("UserSettings", s => UserSettings.Defaults = s);
        _configurationBuilder.ApplyToStaticFacades();
        _configurationBuilder.ThemeManager.SwitchTheme(UserSettings.Defaults.Theme);
    }

    public void Reload()
    {
        if (_configurationBuilder is null) throw new InvalidOperationException($"{nameof(SettingsManager)} was not initialized properly.");

        _configurationBuilder.RuntimeConfig = File.Exists(SettingsFilePath) ? File.ReadAllText(SettingsFilePath) : null;

        _configurationBuilder.BindAppSettings<UserSettings>("UserSettings", s => UserSettings.Defaults = s);
        _configurationBuilder.ApplyToStaticFacades();
        _configurationBuilder.ThemeManager.SwitchTheme(UserSettings.Defaults.Theme);
    }

    public void Apply(UserSettings settings)
    {
        string settingsJson = JsonSerializer.Serialize(new UserSettings.Container() { UserSettings = settings }, UserSettings.SerializerContext.Default.Container);

        Directory.CreateDirectory(_settingsFileDir);
        File.WriteAllText(SettingsFilePath, settingsJson);

        Reload();
    }

    public void SwitchTheme(string themeName)
    {
        if (_configurationBuilder is null) throw new InvalidOperationException($"{nameof(SettingsManager)} was not initialized properly.");

        _configurationBuilder.ThemeManager.SwitchTheme(themeName);
    }
}
