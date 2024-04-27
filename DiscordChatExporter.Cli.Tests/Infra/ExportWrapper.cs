using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AsyncKeyedLock;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using JsonExtensions;

namespace DiscordChatExporter.Cli.Tests.Infra;

public static class ExportWrapper
{
    private static readonly AsyncKeyedLocker<string> Locker =
        new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

    private static readonly string DirPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? Directory.GetCurrentDirectory(),
        "ExportCache"
    );

    static ExportWrapper()
    {
        try
        {
            Directory.Delete(DirPath, true);
        }
        catch (DirectoryNotFoundException) { }

        Directory.CreateDirectory(DirPath);
    }

    private static async ValueTask<string> ExportAsync(Snowflake channelId, ExportFormat format)
    {
        var fileName = channelId.ToString() + '.' + format.GetFileExtension();
        var filePath = Path.Combine(DirPath, fileName);

        using var _ = await Locker.LockAsync(filePath);
        using var console = new FakeConsole();

        // Perform the export only if it hasn't been done before
        if (!File.Exists(filePath))
        {
            await new ExportChannelsCommand
            {
                Token = Secrets.DiscordToken,
                ChannelIds = [channelId],
                ExportFormat = format,
                OutputPath = filePath,
                Locale = "en-US",
                IsUtcNormalizationEnabled = true
            }.ExecuteAsync(console);
        }

        return await File.ReadAllTextAsync(filePath);
    }

    public static async ValueTask<IHtmlDocument> ExportAsHtmlAsync(Snowflake channelId) =>
        Html.Parse(await ExportAsync(channelId, ExportFormat.HtmlDark));

    public static async ValueTask<JsonElement> ExportAsJsonAsync(Snowflake channelId) =>
        Json.Parse(await ExportAsync(channelId, ExportFormat.Json));

    public static async ValueTask<string> ExportAsPlainTextAsync(Snowflake channelId) =>
        await ExportAsync(channelId, ExportFormat.PlainText);

    public static async ValueTask<string> ExportAsCsvAsync(Snowflake channelId) =>
        await ExportAsync(channelId, ExportFormat.Csv);

    public static async ValueTask<IReadOnlyList<IElement>> GetMessagesAsHtmlAsync(
        Snowflake channelId
    ) => (await ExportAsHtmlAsync(channelId)).QuerySelectorAll("[data-message-id]").ToArray();

    public static async ValueTask<IReadOnlyList<JsonElement>> GetMessagesAsJsonAsync(
        Snowflake channelId
    ) => (await ExportAsJsonAsync(channelId)).GetProperty("messages").EnumerateArray().ToArray();

    public static async ValueTask<IElement> GetMessageAsHtmlAsync(
        Snowflake channelId,
        Snowflake messageId
    )
    {
        var message = (await GetMessagesAsHtmlAsync(channelId)).SingleOrDefault(e =>
            string.Equals(
                e.GetAttribute("data-message-id"),
                messageId.ToString(),
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (message is null)
        {
            throw new InvalidOperationException(
                $"Message #{messageId} not found in the export of channel #{channelId}."
            );
        }

        return message;
    }

    public static async ValueTask<JsonElement> GetMessageAsJsonAsync(
        Snowflake channelId,
        Snowflake messageId
    )
    {
        var message = (await GetMessagesAsJsonAsync(channelId)).SingleOrDefault(j =>
            string.Equals(
                j.GetProperty("id").GetString(),
                messageId.ToString(),
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (message.ValueKind == JsonValueKind.Undefined)
        {
            throw new InvalidOperationException(
                $"Message #{messageId} not found in the export of channel #{channelId}."
            );
        }

        return message;
    }
}
