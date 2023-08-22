using System;
using System.Globalization;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class DateExtensions
{
    public static string ToLocalString(this DateTimeOffset instant, string format) =>
        instant.ToLocalTime().ToString(format, CultureInfo.InvariantCulture);
}
