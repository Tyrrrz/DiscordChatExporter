using System;

namespace DiscordChatExporter.Domain.Discord.Models.Common
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
    }
}