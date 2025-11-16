using System.Diagnostics;

namespace DiscordChatExporter.Gui.Utils.Extensions;

internal static class ProcessExtensions
{
    extension(Process)
    {
        public static void StartShellExecute(string path)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo { FileName = path, UseShellExecute = true };

            process.Start();
        }
    }
}
