using System;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class TimeSpanExtensions
{
    public static TimeSpan Clamp(this TimeSpan value, TimeSpan min, TimeSpan max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }
}
