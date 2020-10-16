using System.Windows.Media;

namespace DiscordChatExporter.Gui.Internal
{
    internal static class MediaColor
    {
        public static Color FromHex(string hex) => (Color) ColorConverter.ConvertFromString(hex);
    }
}