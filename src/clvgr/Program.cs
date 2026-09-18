using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static clvgr.Core.Logging;

namespace clvgr;

internal class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RunOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ClipOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PrintOptions))]
    private static void Main(string[] args)
    {
        int BootstrapAndRun(Options opts, Action<IServiceProvider> op)
        {
            using IApplication app = Application.Create();
            using Window window = new() { BorderStyle = LineStyle.None };

            app.Init();

            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
            {
                DisableDefaults = true,
            });

            builder.Services.AddSingleton(app);
            builder.Services.AddSingleton(window);
            builder.Services.AddSingleton<SettingsManager>();
            builder.Services.AddSingleton<AppManager>();
            builder.Services.AddSingleton<SecretsFileManagerFactory>();
            builder.Services.AddSingleton<Func<SecretsFileManager, ResourceList>>(services => sfm => ActivatorUtilities.CreateInstance<ResourceList>(services, sfm));
            builder.Services.RegisterEncryption();
            builder.Services.RegisterClipboard();
            builder.Services.RegisterLogger(opts.LogLevel);

            var host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("Starting application.");
            logger.LogDebug($"Logs location: {LogsFilePath}");
            logger.LogDebug($"Settings location: {SettingsManager.SettingsFilePath}");

            try
            {
                op(host.Services);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error.");
                Console.WriteLine($"Unhandled error. See the logs file for more details: {LogsFilePath}.");
            }

            return 0;
        }

        Parser.Default.ParseArguments<RunOptions, ClipOptions, PrintOptions>(args)
            .MapResult(
                (RunOptions opts) => BootstrapAndRun(opts, sp => Verbs.Run(opts, sp)),
                (ClipOptions opts) => BootstrapAndRun(opts, sp => Verbs.Clip(opts, sp)),
                (PrintOptions opts) => BootstrapAndRun(opts, sp => Verbs.Print(opts, sp)),
                errs => 0
            );
    }
}