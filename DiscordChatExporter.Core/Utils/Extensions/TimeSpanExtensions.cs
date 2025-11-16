using System;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class TimeSpanExtensions
{
    extension(TimeSpan value)
    {
        public TimeSpan Clamp(TimeSpan min, TimeSpan max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
