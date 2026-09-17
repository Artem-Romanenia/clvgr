using Microsoft.Extensions.DependencyInjection;
using static clvgr.Core.Clipboard;

namespace clvgr.Cli;

internal static class Verbs
{
    public static void Run(Options opts, IServiceProvider provider)
    {
        var appManager = provider.GetRequiredService<AppManager>();

        if ((opts.Path ?? UserSettings.Defaults.LastSecretsFile) is string path)
        {
            appManager.InitializeSecretsFileManager(path);
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
