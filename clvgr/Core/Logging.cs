using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace clvgr.Core;

internal static class Logging
{
    public static void RegisterLogger(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(LogsFilePath)
            .CreateLogger();

        services.AddSerilog();
    }

    public static string LogsFilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "clvgr", "logs.txt");
}
