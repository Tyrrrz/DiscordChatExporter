using System;
using System.Globalization;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class DateExtensions
{
    public static string ToLocalString(this DateTimeOffset dateTime, string format) =>
        dateTime.ToLocalTime().ToString(format, CultureInfo.InvariantCulture);
}