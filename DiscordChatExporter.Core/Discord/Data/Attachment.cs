using System;
using System.IO;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#attachment-object
public partial record Attachment(
    Snowflake Id,
    string Url,
    string FileName,
    string? Description,
    int? Width,
    int? Height,
    FileSize FileSize
) : IHasId
{
    public string FileExtension => Path.GetExtension(FileName);

    public bool IsImage =>
        string.Equals(FileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".png", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".bmp", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".webp", StringComparison.OrdinalIgnoreCase);

    public bool IsVideo =>
        string.Equals(FileExtension, ".gifv", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".mp4", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".webm", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".mov", StringComparison.OrdinalIgnoreCase);

    public bool IsAudio =>
        string.Equals(FileExtension, ".mp3", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".wav", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".ogg", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".flac", StringComparison.OrdinalIgnoreCase)
        || string.Equals(FileExtension, ".m4a", StringComparison.OrdinalIgnoreCase);

    public bool IsSpoiler => FileName.StartsWith("SPOILER_", StringComparison.Ordinal);
}

public partial record Attachment
{
    public static Attachment Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var url = json.GetProperty("url").GetNonWhiteSpaceString();
        var fileName = json.GetProperty("filename").GetNonNullString();
        var description = json.GetPropertyOrNull("description")?.GetNonWhiteSpaceStringOrNull();
        var width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
        var height = json.GetPropertyOrNull("height")?.GetInt32OrNull();
        var fileSize = json.GetProperty("size").GetInt64().Pipe(FileSize.FromBytes);

        return new Attachment(id, url, fileName, description, width, height, fileSize);
    }
}
