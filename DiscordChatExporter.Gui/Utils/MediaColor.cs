using System.Windows.Media;

namespace DiscordChatExporter.Gui.Utils;

internal static class MediaColor
{
    public static Color FromHex(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
