using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Exporting.Partitioning;

public abstract partial class PartitionLimit
{
    public abstract bool IsReached(long messagesWritten, long bytesWritten);
}

public partial class PartitionLimit
{
    public static PartitionLimit Null { get; } = new NullPartitionLimit();

    private static long? TryParseFileSizeBytes(string value, IFormatProvider? formatProvider = null)
    {
        var match = Regex.Match(value, @"^\s*(\d+[\.,]?\d*)\s*(\w)?b\s*$", RegexOptions.IgnoreCase);

        // Number part
        if (
            !double.TryParse(
                match.Groups[1].Value,
                NumberStyles.Float,
                formatProvider,
                out var number
            )
        )
        {
            return null;
        }

        // Magnitude part
        var magnitude = match.Groups[2].Value.ToUpperInvariant() switch
        {
            "G" => 1_000_000_000,
            "M" => 1_000_000,
            "K" => 1_000,
            "" => 1,
            _ => -1
        };

        if (magnitude < 0)
        {
            return null;
        }

        return (long)(number * magnitude);
    }

    public static PartitionLimit? TryParse(string value, IFormatProvider? formatProvider = null)
    {
        var fileSizeLimit = TryParseFileSizeBytes(value, formatProvider);
        if (fileSizeLimit is not null)
            return new FileSizePartitionLimit(fileSizeLimit.Value);

        if (int.TryParse(value, NumberStyles.Integer, formatProvider, out var messageCountLimit))
            return new MessageCountPartitionLimit(messageCountLimit);

        return null;
    }

    public static PartitionLimit Parse(string value, IFormatProvider? formatProvider = null) =>
        TryParse(value, formatProvider)
        ?? throw new FormatException($"Invalid partition limit '{value}'.");
}
