using System.Drawing;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class ColorExtensions
    {
        public static Color WithAlpha(this Color color, int alpha) => Color.FromArgb(alpha, color);

        public static Color ResetAlpha(this Color color) => color.WithAlpha(255);

        public static int ToRgb(this Color color) => color.ToArgb() & 0xffffff;
    }
}