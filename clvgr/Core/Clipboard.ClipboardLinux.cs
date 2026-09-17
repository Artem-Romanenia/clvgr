using clvgr.Core.Exceptions;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace clvgr.Core;

internal partial class Clipboard
{
    [SupportedOSPlatform("linux")]
    private class ClipboardLinux : IClipboardAccessor
    {
        public void PutToClipboard(ReadOnlySpan<byte> utf8Bytes)
        {
            if (IsWsl())
            {
                WriteToProcess("powershell.exe", "-NoProfile -Command \"[Console]::In.ReadToEnd() | Set-Clipboard\"", utf8Bytes);
            }

            if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is { })
            {
                if (BinaryExists("wl-copy"))
                {
                    WriteToProcess("wl-copy", "--type text/plain", utf8Bytes);
                }
            }

            if (BinaryExists("xclip"))
            {
                WriteToProcess("xclip", "-selection clipboard -in", utf8Bytes);
            }

            if (BinaryExists("xsel"))
            {
                WriteToProcess("xsel", "--clipboard --input", utf8Bytes);
            }
        }

        private static void WriteToProcess(string fileName, string arguments, ReadOnlySpan<byte> utf8Bytes)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process is null) throw new ClipboardException($"Failed to start '{fileName}'.");

                using (var baseStream = process.StandardInput.BaseStream)
                {
                    baseStream.Write(utf8Bytes);
                    baseStream.Flush();
                }

                process.WaitForExit(TimeSpan.FromSeconds(2));
                if (process.ExitCode != 0)
                {
                    throw new ClipboardException($"Process '{fileName}' exited with code {process.ExitCode}.");
                }
            }
            catch (Exception ex)
            {
                throw new ClipboardException($"Failed to run '{fileName}'.", ex);
            }
        }

        private static bool BinaryExists(string name)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = name,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p is null) return false;
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsWsl()
        {
            try
            {
                if (File.Exists("/proc/version"))
                {
                    string versionInfo = File.ReadAllText("/proc/version");
                    return versionInfo.Contains("Microsoft", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
            }
            return false;
        }
    }
}
