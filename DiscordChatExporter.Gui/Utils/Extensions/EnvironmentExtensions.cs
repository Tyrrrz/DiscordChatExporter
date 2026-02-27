using System;
using System.IO;

namespace DiscordChatExporter.Gui.Utils.Extensions;

internal static class EnvironmentExtensions
{
    extension(Environment)
    {
        // Returns a stable machine-specific identifier for key derivation.
        // This makes a stolen settings file non-trivially decryptable on a different machine.
        public static string GetMachineId()
        {
            // Windows: stable GUID written during OS installation
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    using var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Cryptography"
                    );
                    if (
                        regKey?.GetValue("MachineGuid") is string guid
                        && !string.IsNullOrWhiteSpace(guid)
                    )
                        return guid;
                }
                catch { }
            }

            // Linux: /etc/machine-id (set once by systemd at first boot)
            foreach (var path in new[] { "/etc/machine-id", "/var/lib/dbus/machine-id" })
            {
                try
                {
                    var id = File.ReadAllText(path).Trim();
                    if (!string.IsNullOrWhiteSpace(id))
                        return id;
                }
                catch { }
            }

            // Last-resort fallback (e.g. macOS without /etc/machine-id)
            return Environment.MachineName;
        }
    }
}
