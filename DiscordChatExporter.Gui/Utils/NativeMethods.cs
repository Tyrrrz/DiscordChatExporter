using System.Runtime.InteropServices;

namespace DiscordChatExporter.Gui.Utils;

internal static class NativeMethods
{
    public static class Windows
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int MessageBox(nint hWnd, string text, string caption, uint type);
    }

    public static class MacOS
    {
        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        public static extern void NSRunAlertPanel(string title, string message, string defaultButton, string alternateButton, string otherButton);
    }
}
