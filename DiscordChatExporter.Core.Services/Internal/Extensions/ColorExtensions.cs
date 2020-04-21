using System.Drawing;

namespace DiscordChatExporter.Core.Services.Internal.Extensions
{
    internal static class ColorExtensions
    {
        public static Color ResetAlpha(this Color color) => Color.FromArgb(1, color);
    }
}