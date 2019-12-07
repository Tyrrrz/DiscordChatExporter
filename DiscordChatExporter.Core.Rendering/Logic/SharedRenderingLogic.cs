using System;
using System.Globalization;

namespace DiscordChatExporter.Core.Rendering.Logic
{
    public static class SharedRenderingLogic
    {
        public static string FormatDate(DateTimeOffset date, string dateFormat) =>
            date.ToLocalTime().ToString(dateFormat, CultureInfo.InvariantCulture);
    }
}