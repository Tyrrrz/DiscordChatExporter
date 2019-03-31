using System;

namespace DiscordChatExporter.Core.Models
{
    // Loosely based on https://github.com/omar/ByteSize (MIT license)

    public struct FileSize
    {
        public const long BytesInKiloByte = 1024;
        public const long BytesInMegaByte = 1024 * BytesInKiloByte;
        public const long BytesInGigaByte = 1024 * BytesInMegaByte;
        public const long BytesInTeraByte = 1024 * BytesInGigaByte;
        public const long BytesInPetaByte = 1024 * BytesInTeraByte;

        public const string ByteSymbol = "B";
        public const string KiloByteSymbol = "KB";
        public const string MegaByteSymbol = "MB";
        public const string GigaByteSymbol = "GB";
        public const string TeraByteSymbol = "TB";
        public const string PetaByteSymbol = "PB";

        public double Bytes { get; }
        public double KiloBytes => Bytes / BytesInKiloByte;
        public double MegaBytes => Bytes / BytesInMegaByte;
        public double GigaBytes => Bytes / BytesInGigaByte;
        public double TeraBytes => Bytes / BytesInTeraByte;
        public double PetaBytes => Bytes / BytesInPetaByte;

        public string LargestWholeNumberSymbol
        {
            get
            {
                // Absolute value is used to deal with negative values
                if (Math.Abs(PetaBytes) >= 1)
                    return PetaByteSymbol;

                if (Math.Abs(TeraBytes) >= 1)
                    return TeraByteSymbol;

                if (Math.Abs(GigaBytes) >= 1)
                    return GigaByteSymbol;

                if (Math.Abs(MegaBytes) >= 1)
                    return MegaByteSymbol;

                if (Math.Abs(KiloBytes) >= 1)
                    return KiloByteSymbol;

                return ByteSymbol;
            }
        }

        public double LargestWholeNumberValue
        {
            get
            {
                // Absolute value is used to deal with negative values
                if (Math.Abs(PetaBytes) >= 1)
                    return PetaBytes;

                if (Math.Abs(TeraBytes) >= 1)
                    return TeraBytes;

                if (Math.Abs(GigaBytes) >= 1)
                    return GigaBytes;

                if (Math.Abs(MegaBytes) >= 1)
                    return MegaBytes;

                if (Math.Abs(KiloBytes) >= 1)
                    return KiloBytes;

                return Bytes;
            }
        }

        public FileSize(double bytes)
        {
            Bytes = bytes;
        }

        public override string ToString() => $"{LargestWholeNumberValue:0.##} {LargestWholeNumberSymbol}";
    }
}