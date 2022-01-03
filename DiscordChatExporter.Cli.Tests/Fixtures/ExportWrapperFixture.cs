using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using JsonExtensions;

namespace DiscordChatExporter.Cli.Tests.Fixtures;

public class ExportWrapperFixture : IDisposable
{
    private string DirPath { get; } = Path.Combine(
        Path.GetDirectoryName(typeof(ExportWrapperFixture).Assembly.Location) ?? Directory.GetCurrentDirectory(),
        "ExportCache",
        Guid.NewGuid().ToString()
    );

    public ExportWrapperFixture() => DirectoryEx.Reset(DirPath);

    private async ValueTask<string> ExportAsync(Snowflake channelId, ExportFormat format)
    {
        var fileName = channelId.ToString() + '.' + format.GetFileExtension();
        var filePath = Path.Combine(DirPath, fileName);

        // Perform export only if it hasn't been done before
        if (!File.Exists(filePath))
        {
            await new ExportChannelsCommand
            {
                Token = Secrets.DiscordToken,
                ChannelIds = new[] { channelId },
                ExportFormat = format,
                OutputPath = filePath
            }.ExecuteAsync(new FakeConsole());
        }

        return await File.ReadAllTextAsync(filePath);
    }

    public async ValueTask<IHtmlDocument> ExportAsHtmlAsync(Snowflake channelId)
    {
        var data = await ExportAsync(channelId, ExportFormat.HtmlDark);
        return Html.Parse(data);
    }

    public async ValueTask<JsonElement> ExportAsJsonAsync(Snowflake channelId)
    {
        var data = await ExportAsync(channelId, ExportFormat.Json);
        return Json.Parse(data);
    }

    public async ValueTask<string> ExportAsPlainTextAsync(Snowflake channelId)
    {
        var data = await ExportAsync(channelId, ExportFormat.PlainText);
        return data;
    }

    public async ValueTask<string> ExportAsCsvAsync(Snowflake channelId)
    {
        var data = await ExportAsync(channelId, ExportFormat.Csv);
        return data;
    }

    public async ValueTask<IReadOnlyList<IElement>> GetMessagesAsHtmlAsync(Snowflake channelId)
    {
        var document = await ExportAsHtmlAsync(channelId);
        return document.QuerySelectorAll("[data-message-id]").ToArray();
    }

    public async ValueTask<IReadOnlyList<JsonElement>> GetMessagesAsJsonAsync(Snowflake channelId)
    {
        var document = await ExportAsJsonAsync(channelId);
        return document.GetProperty("messages").EnumerateArray().ToArray();
    }

    public async ValueTask<IElement> GetMessageAsHtmlAsync(Snowflake channelId, Snowflake messageId)
    {
        var messages = await GetMessagesAsHtmlAsync(channelId);

        var message = messages.SingleOrDefault(e =>
            string.Equals(
                e.GetAttribute("data-message-id"),
                messageId.ToString(),
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (message is null)
        {
            throw new InvalidOperationException(
                $"Message '{messageId}' does not exist in export of channel '{channelId}'."
            );
        }

        return message;
    }

    public async ValueTask<JsonElement> GetMessageAsJsonAsync(Snowflake channelId, Snowflake messageId)
    {
        var messages = await GetMessagesAsJsonAsync(channelId);

        var message = messages.FirstOrDefault(j =>
            string.Equals(
                j.GetProperty("id").GetString(),
                messageId.ToString(),
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (message.ValueKind == JsonValueKind.Undefined)
        {
            throw new InvalidOperationException(
                $"Message '{messageId}' does not exist in export of channel '{channelId}'."
            );
        }

        return message;
    }

    public void Dispose() => DirectoryEx.DeleteIfExists(DirPath);
}