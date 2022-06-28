using System;
using System.IO;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils;
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
    FileSize FileSize) : IHasId
{
    public string FileExtension => Path.GetExtension(FileName);

    public bool IsImage => FileFormat.IsImage(FileExtension);

    public bool IsVideo => FileFormat.IsVideo(FileExtension);

    public bool IsAudio => FileFormat.IsAudio(FileExtension);

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