using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DiscordChatExporter.Core.Discord.Data.Common;

// Loosely based on https://github.com/omar/ByteSize (MIT license)
public readonly partial record struct FileSize(long TotalBytes)
{
    public double TotalKiloBytes => TotalBytes / 1024.0;
    public double TotalMegaBytes => TotalKiloBytes / 1024.0;
    public double TotalGigaBytes => TotalMegaBytes / 1024.0;

    private double GetLargestWholeNumberValue()
    {
        if (Math.Abs(TotalGigaBytes) >= 1)
            return TotalGigaBytes;

        if (Math.Abs(TotalMegaBytes) >= 1)
            return TotalMegaBytes;

        if (Math.Abs(TotalKiloBytes) >= 1)
            return TotalKiloBytes;

        return TotalBytes;
    }

    private string GetLargestWholeNumberSymbol()
    {
        if (Math.Abs(TotalGigaBytes) >= 1)
            return "GB";

        if (Math.Abs(TotalMegaBytes) >= 1)
            return "MB";

        if (Math.Abs(TotalKiloBytes) >= 1)
            return "KB";

        return "bytes";
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{GetLargestWholeNumberValue():0.##} {GetLargestWholeNumberSymbol()}"
        );
}

public partial record struct FileSize
{
    public static FileSize FromBytes(long bytes) => new(bytes);
}
