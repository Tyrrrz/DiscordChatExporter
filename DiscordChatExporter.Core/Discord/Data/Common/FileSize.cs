using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Common
{
    // Loosely based on https://github.com/omar/ByteSize (MIT license)
    public readonly partial struct FileSize
    {
        public long TotalBytes { get; }

        public double TotalKiloBytes => TotalBytes / 1024.0;
        public double TotalMegaBytes => TotalKiloBytes / 1024.0;
        public double TotalGigaBytes => TotalMegaBytes / 1024.0;
        public double TotalTeraBytes => TotalGigaBytes / 1024.0;
        public double TotalPetaBytes => TotalTeraBytes / 1024.0;

        public FileSize(long bytes) => TotalBytes = bytes;

        private double GetLargestWholeNumberValue()
        {
            if (Math.Abs(TotalPetaBytes) >= 1)
                return TotalPetaBytes;

            if (Math.Abs(TotalTeraBytes) >= 1)
                return TotalTeraBytes;

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
            if (Math.Abs(TotalPetaBytes) >= 1)
                return "PB";

            if (Math.Abs(TotalTeraBytes) >= 1)
                return "TB";

            if (Math.Abs(TotalGigaBytes) >= 1)
                return "GB";

            if (Math.Abs(TotalMegaBytes) >= 1)
                return "MB";

            if (Math.Abs(TotalKiloBytes) >= 1)
                return "KB";

            return "bytes";
        }

        public override string ToString() => $"{GetLargestWholeNumberValue():0.##} {GetLargestWholeNumberSymbol()}";
    }

    public partial struct FileSize
    {
        public static FileSize FromBytes(long bytes) => new(bytes);

        public static FileSize? TryParse(string value)
        {
            var match = Regex.Match(value, @"^(\d+[\.,]?\d*)\s*(\w)?b$", RegexOptions.IgnoreCase);

            // Number part
            if (!double.TryParse(
                match.Groups[1].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var number))
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

            return FromBytes((long) (number * magnitude));
        }
    }
}