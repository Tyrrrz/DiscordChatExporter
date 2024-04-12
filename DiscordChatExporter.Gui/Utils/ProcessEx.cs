using System.Diagnostics;

namespace DiscordChatExporter.Gui.Utils;

internal static class ProcessEx
{
    public static void StartShellExecute(string path)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo { FileName = path, UseShellExecute = true };

        process.Start();
    }
}
