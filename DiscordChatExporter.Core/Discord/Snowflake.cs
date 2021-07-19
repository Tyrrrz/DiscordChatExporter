using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord
{
    public readonly partial struct Snowflake
    {
        public ulong Value { get; }

        public Snowflake(ulong value) => Value = value;

        public DateTimeOffset ToDate() => DateTimeOffset.FromUnixTimeMilliseconds(
            (long) ((Value >> 22) + 1420070400000UL)
        ).ToLocalTime();

        [ExcludeFromCodeCoverage]
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public partial struct Snowflake
    {
        public static Snowflake Zero { get; } = new(0);

        public static Snowflake FromDate(DateTimeOffset date)
        {
            var value = ((ulong) date.ToUnixTimeMilliseconds() - 1420070400000UL) << 22;
            return new Snowflake(value);
        }

        public static Snowflake? TryParse(string? str, IFormatProvider? formatProvider = null)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            // As number
            if (Regex.IsMatch(str, @"^\d+$") &&
                ulong.TryParse(str, NumberStyles.Number, formatProvider, out var value))
            {
                return new Snowflake(value);
            }

            // As date
            if (DateTimeOffset.TryParse(str, formatProvider, DateTimeStyles.None, out var date))
            {
                return FromDate(date);
            }

            return null;
        }

        public static Snowflake Parse(string str, IFormatProvider? formatProvider) =>
            TryParse(str, formatProvider) ?? throw new FormatException($"Invalid snowflake '{str}'.");

        public static Snowflake Parse(string str) => Parse(str, null);
    }

    public partial struct Snowflake : IComparable<Snowflake>, IEquatable<Snowflake>
    {
        public int CompareTo(Snowflake other) => Value.CompareTo(other.Value);

        public bool Equals(Snowflake other) => CompareTo(other) == 0;

        public override bool Equals(object? obj) => obj is Snowflake other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Snowflake left, Snowflake right) => left.Equals(right);

        public static bool operator !=(Snowflake left, Snowflake right) => !(left == right);
    }
}