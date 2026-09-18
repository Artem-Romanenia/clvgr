using Microsoft.Extensions.DependencyInjection;
using static clvgr.Core.Clipboard;

namespace clvgr.Cli;

internal static class Verbs
{
    public static void Run(RunOptions opts, IServiceProvider provider)
    {
        var appManager = provider.GetRequiredService<AppManager>();

        // If user deliberately passed the file path, proceed with entering password
        if (opts.Path is string optionsPath)
        {
            appManager.InitializeSecretsFileManager(optionsPath);
        }
        // If Last Secret file was remembered but doesn't exist anymore, ignore
        else if (UserSettings.Defaults.LastSecretsFile is string preferencePath && File.Exists(preferencePath))
        {
            appManager.InitializeSecretsFileManager(preferencePath);
        }

        appManager.RunMainWindow();
    }

    public static void Clip(ClipOptions opts, IServiceProvider provider)
    {
        var appManager = provider.GetRequiredService<AppManager>();

        if ((opts.Path ?? UserSettings.Defaults.LastSecretsFile) is string path)
        {
            appManager.InitializeSecretsFileManager(Path.GetFullPath(path));
        }
        else
        {
            appManager.SelectSecretsFile();
        }

        if (appManager.CurrentSecretsFileManager is { Resources: var resources, Cryptor: var cryptor })
        {
            var resource = resources.FirstOrDefault(s => s.ShortName == opts.ShortName);

            if (resource is { })
            {
                var clipboardAccessor = provider.GetRequiredService<IClipboardAccessor>();

                cryptor.Decrypt(resource.Secret.ToArray(), clipboardAccessor.PutToClipboard);
            }
        }
    }

    public static void Print(PrintOptions opts, IServiceProvider provider)
    {
        var appManager = provider.GetRequiredService<AppManager>();

        if ((opts.Path ?? UserSettings.Defaults.LastSecretsFile) is string path)
        {
            appManager.InitializeSecretsFileManager(Path.GetFullPath(path));
        }
        else
        {
            appManager.SelectSecretsFile();
        }

        if (appManager.CurrentSecretsFileManager is { Resources: var resources, Cryptor: var cryptor })
        {
            var resource = resources.FirstOrDefault(s => s.ShortName == opts.ShortName);

            if (resource is { })
            {
                cryptor.Decrypt(resource.Secret.ToArray(), plaintext => { });
            }
        }
    }
}
