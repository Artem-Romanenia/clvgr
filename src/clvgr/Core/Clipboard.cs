using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace clvgr.Core;

internal static partial class Clipboard
{
    public static void RegisterClipboard(this IServiceCollection services)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            services.AddSingleton<IClipboardAccessor, ClipboardLinux>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddSingleton<IClipboardAccessor, ClipboardWindows>();
        }
        else throw new NotSupportedException($"{RuntimeInformation.OSDescription} is not supported.");
    }
}
