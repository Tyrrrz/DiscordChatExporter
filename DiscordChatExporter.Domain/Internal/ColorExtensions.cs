using System.Drawing;

namespace DiscordChatExporter.Domain.Internal
{
    internal static class ColorExtensions
    {
        public static Color ResetAlpha(this Color color) => Color.FromArgb(1, color);

        public static int ToRgb(this Color color) => color.ToArgb() & 0xffffff;

        public static string ToHexString(this Color color) => $"#{color.ToRgb():x6}";

        public static string ToRgbString(this Color color) => $"{color.R}, {color.G}, {color.B}";
    }
}