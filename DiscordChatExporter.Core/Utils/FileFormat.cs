using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Utils;

public static class FileFormat
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".gifv",
        ".bmp",
        ".webp"
    };

    public static bool IsImage(string format) => ImageExtensions.Contains(format);

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4",
        ".webm",
        ".mov"
    };

    public static bool IsVideo(string format) => VideoExtensions.Contains(format);

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".ogg",
        ".flac",
        ".m4a"
    };

    public static bool IsAudio(string format) => AudioExtensions.Contains(format);
}