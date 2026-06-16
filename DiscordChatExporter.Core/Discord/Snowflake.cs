using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DiscordChatExporter.Core.Discord;

public readonly partial record struct Snowflake(ulong Value)
{
    public DateTimeOffset ToDate() =>
        DateTimeOffset
            .FromUnixTimeMilliseconds((long)((Value >> 22) + 1420070400000UL))
            .ToLocalTime();

    [ExcludeFromCodeCoverage]
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

public partial record struct Snowflake
{
    public static Snowflake Zero { get; } = new(0);

    public static Snowflake FromDate(DateTimeOffset instant) =>
        new(((ulong)instant.ToUnixTimeMilliseconds() - 1420070400000UL) << 22);

    public static Snowflake? TryParse(string? value, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // As number
        if (ulong.TryParse(value, NumberStyles.None, formatProvider, out var number))
            return new Snowflake(number);

        // As date
        if (DateTimeOffset.TryParse(value, formatProvider, DateTimeStyles.None, out var instant))
            return FromDate(instant);

        return null;
    }

    public static Snowflake Parse(string value, IFormatProvider? formatProvider) =>
        TryParse(value, formatProvider)
        ?? throw new FormatException($"Invalid snowflake '{value}'.");

    public static Snowflake Parse(string value) => Parse(value, null);
}

public partial record struct Snowflake : IComparable<Snowflake>, IComparable
{
    public int CompareTo(Snowflake other) => Value.CompareTo(other.Value);

    public int CompareTo(object? obj)
    {
        if (obj is not Snowflake other)
            throw new ArgumentException($"Object must be of type {nameof(Snowflake)}.");

        return Value.CompareTo(other.Value);
    }

    public static bool operator >(Snowflake left, Snowflake right) => left.CompareTo(right) > 0;

    public static bool operator <(Snowflake left, Snowflake right) => left.CompareTo(right) < 0;
}
