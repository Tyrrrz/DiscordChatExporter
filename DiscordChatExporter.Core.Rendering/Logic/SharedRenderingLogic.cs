using System;
using System.Globalization;

namespace DiscordChatExporter.Core.Rendering.Logic
{
    public static class SharedRenderingLogic
    {
        public static string FormatDate(DateTimeOffset date, string dateFormat, bool isUseUtcEnabled) =>
            (isUseUtcEnabled ? date.ToUniversalTime() : date.ToLocalTime()).ToString(dateFormat, CultureInfo.InvariantCulture);
    }
}