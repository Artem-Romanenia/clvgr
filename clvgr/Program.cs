using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static clvgr.Core.Logging;

namespace clvgr;

internal class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Options))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ClipOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PrintOptions))]
    private static void Main(string[] args)
    {
        int BootstrapAndRun(Action<IServiceProvider> op)
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
            builder.Services.RegisterLogger();

            var host = builder.Build();

            host.Services.GetRequiredService<ILogger<Program>>().LogInformation("Startred.");

            try
            {
                op(host.Services);
            }
            catch (Exception ex)
            {
                host.Services.GetRequiredService<ILogger>().LogError(ex, "Unhandled error.");
                Console.WriteLine($"Unhandled error. See the logs file for more details: {LogsFilePath}.");
            }

            return 0;
        }

        Parser.Default.ParseArguments<Options, ClipOptions, PrintOptions>(args)
            .MapResult(
                (Options opts) => BootstrapAndRun(sp => Verbs.Run(opts, sp)),
                (ClipOptions opts) => BootstrapAndRun(sp => Verbs.Clip(opts, sp)),
                (PrintOptions opts) => BootstrapAndRun(sp => Verbs.Print(opts, sp)),
                errs => 0
            );
    }
}