using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Dump;

public partial class DataDump(IReadOnlyList<DataDumpChannel> channels)
{
    public IReadOnlyList<DataDumpChannel> Channels { get; } = channels;
}

public partial class DataDump
{
    public static DataDump Parse(JsonElement json)
    {
        var channels = new List<DataDumpChannel>();

        foreach (var property in json.EnumerateObjectOrEmpty())
        {
            var channelId = Snowflake.Parse(property.Name);
            var channelName = property.Value.GetString();

            // Null items refer to deleted channels
            if (channelName is null)
                continue;

            var channel = new DataDumpChannel(channelId, channelName);
            channels.Add(channel);
        }

        return new DataDump(channels);
    }

    public static async ValueTask<DataDump> LoadAsync(
        string zipFilePath,
        CancellationToken cancellationToken = default
    )
    {
        using var archive = ZipFile.OpenRead(zipFilePath);

        var entry = archive.GetEntry("messages/index.json");
        if (entry is null)
        {
            throw new InvalidOperationException(
                "Failed to locate the channel index inside the data package."
            );
        }

        await using var stream = entry.Open();
        using var document = await JsonDocument.ParseAsync(stream, default, cancellationToken);

        return Parse(document.RootElement);
    }
}
