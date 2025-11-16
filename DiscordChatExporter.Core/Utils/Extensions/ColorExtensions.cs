using System.Drawing;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class ColorExtensions
{
    extension(Color color)
    {
        public Color WithAlpha(int alpha) => Color.FromArgb(alpha, color);

        public Color ResetAlpha() => color.WithAlpha(255);

        public int ToRgb() => color.ToArgb() & 0xffffff;

        public string ToHex() => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
