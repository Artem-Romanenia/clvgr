using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace clvgr.Core;

internal static partial class Encryption
{
    public static void RegisterEncryption(this IServiceCollection services)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            services.AddSingleton<ICryptorFactory, CryptorFactoryLinux>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddSingleton<ICryptorFactory, CryptorFactoryWindows>();
        }
        else throw new NotSupportedException($"{RuntimeInformation.OSDescription} is not supported.");
    }
}
