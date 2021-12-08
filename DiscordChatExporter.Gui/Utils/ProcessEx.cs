using System.Diagnostics;

namespace DiscordChatExporter.Gui.Utils;

internal static class ProcessEx
{
    public static void StartShellExecute(string path)
    {
        var startInfo = new ProcessStartInfo(path)
        {
            UseShellExecute = true
        };

        using (Process.Start(startInfo)) {}
    }
}