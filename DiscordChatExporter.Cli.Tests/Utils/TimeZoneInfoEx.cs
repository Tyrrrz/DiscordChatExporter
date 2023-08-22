using System;
using ReflectionMagic;

namespace DiscordChatExporter.Cli.Tests.Utils;

internal static class TimeZoneInfoEx
{
    // https://stackoverflow.com/a/63700512/2205454
    public static void SetLocal(TimeZoneInfo timeZone) =>
        typeof(TimeZoneInfo).AsDynamicType().s_cachedData._localTimeZone = timeZone;

    public static void SetLocal(TimeSpan offset) =>
        SetLocal(TimeZoneInfo.CreateCustomTimeZone("test-tz", offset, "test-tz", "test-tz"));
}
