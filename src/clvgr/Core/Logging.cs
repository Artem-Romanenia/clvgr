using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace clvgr.Core;

internal static class Logging
{
    public static void RegisterLogger(this IServiceCollection services, LogEventLevel? logEventLevel = null)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logEventLevel ?? LogEventLevel.Debug)
            .WriteTo.File(LogsFilePath)
            .CreateLogger();

        services.AddSerilog();
    }

    public static string LogsFilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "clvgr", "logs.txt");
}
