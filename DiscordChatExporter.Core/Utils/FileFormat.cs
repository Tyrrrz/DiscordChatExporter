using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Utils;

public static class FileFormat
{
    private static readonly HashSet<string> ImageFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".gifv",
        ".bmp",
        ".webp"
    };

    public static bool IsImage(string format) => ImageFormats.Contains(format);

    private static readonly HashSet<string> VideoFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4",
        ".webm",
        ".mov"
    };

    public static bool IsVideo(string format) => VideoFormats.Contains(format);

    private static readonly HashSet<string> AudioFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".ogg",
        ".flac",
        ".m4a"
    };

    public static bool IsAudio(string format) => AudioFormats.Contains(format);
}