using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using PowerKit;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlUpdateSpecs
{
    private const string Original = """
        <!DOCTYPE html><html><body>
        <div class="chatlog" data-channel-id="42">
        <div class=chatlog__message-group><div id=chatlog__message-container-1 data-message-id=1>old first</div></div>
        <div class=chatlog__message-group><div id=chatlog__message-container-2 data-message-id=2>old second</div></div>
        </div>
        <div class=postamble><div class=postamble__entry>Exported 2 message(s)</div><div class=postamble__entry>Timezone: UTC+0</div></div>

        """;

    private static ExportRequest CreateRequest(string path, ulong? lastId = 3)
    {
        var channel = new Channel(
            new Snowflake(42),
            ChannelKind.DirectTextChat,
            Guild.DirectMessages.Id,
            null,
            "Test",
            null,
            null,
            null,
            false,
            lastId is { } id ? new Snowflake(id) : null
        );

        return new ExportRequest(
            Guild.DirectMessages,
            channel,
            path,
            null,
            ExportFormat.HtmlDark,
            null,
            null,
            PartitionLimit.Null,
            MessageFilter.Null,
            false,
            false,
            false,
            false,
            "en-US",
            false,
            true
        );
    }

    private sealed class DiscordHandler(
        bool failMessages = false,
        bool truncate = false,
        ulong? channelLastId = 3,
        bool failChannel = false,
        int lastExistingMessageId = 3
    ) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var uri = request.RequestUri!;
            Requests.Add(uri.PathAndQuery);
            var result = uri.AbsolutePath switch
            {
                "/api/v10/users/@me" => "{}",
                "/api/v10/users/@me/channels" => "[]",
                "/api/v10/channels/42" =>
                    $$"""{"id":"42","type":1,"last_message_id":{{(channelLastId is { } id ? $"\"{id}\"" : "null")}}}""",
                "/api/v10/channels/42/messages" when truncate && uri.Query.Contains("after=2") =>
                    "[]",
                "/api/v10/channels/42/messages" when uri.Query.Contains("after=2") =>
                    lastExistingMessageId > 2
                        ? $"[{Message(lastExistingMessageId, "new third")}]"
                        : "[]",
                "/api/v10/channels/42/messages" when uri.Query.Contains("after=3") => "[]",
                "/api/v10/channels/42/messages" when uri.Query.Contains("limit=1") =>
                    $"[{Message(lastExistingMessageId, "new third")}]",
                _ => throw new InvalidOperationException($"Unexpected request: {uri.PathAndQuery}"),
            };

            var status =
                (failChannel && uri.AbsolutePath == "/api/v10/channels/42")
                || (
                    failMessages
                    && uri.AbsolutePath.EndsWith("/messages")
                    && uri.Query.Contains("limit=100")
                )
                    ? HttpStatusCode.Forbidden
                    : HttpStatusCode.OK;

            return Task.FromResult(
                new HttpResponseMessage(status) { Content = new StringContent(result) }
            );
        }

        private static string Message(int id, string content) =>
            $$"""
                {"id":"{{id}}","type":0,"author":{"id":"99","username":"tester"},"timestamp":"2025-01-01T00:00:00+00:00","content":"{{content}}"}
                """;
    }

    private static ChannelExporter CreateExporter(DiscordHandler handler) =>
        new(
            new DiscordClient("test-token", RateLimitPreference.IgnoreAll, new HttpClient(handler))
        );

    [Theory]
    [InlineData(2UL)]
    [InlineData(null)]
    public async Task Append_refreshes_stale_channel_metadata(ulong? lastId)
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);
        var handler = new DiscordHandler();

        await CreateExporter(handler).ExportChannelAsync(CreateRequest(path, lastId));

        (await File.ReadAllTextAsync(path)).Should().Contain("new third");
        handler.Requests.Should().Contain("/api/v10/channels/42");
    }

    [Fact]
    public async Task Append_preserves_old_messages_and_fetches_only_after_the_last_id()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);
        var handler = new DiscordHandler();

        await CreateExporter(handler).ExportChannelAsync(CreateRequest(path));

        var html = await File.ReadAllTextAsync(path);
        html.Should().Contain("old first").And.Contain("old second").And.Contain("new third");
        html.Should().Contain("Exported 3 message(s)");
        Html.Parse(html).QuerySelectorAll(".chatlog__message-group").Should().HaveCount(3);
        handler.Requests.Should().Contain(s => s.Contains("limit=100") && s.Contains("after=2"));
        handler.Requests.Should().NotContain(s => s.Contains("limit=100") && s.Contains("after=0"));
        var backup = Directory
            .GetFiles(dir.Path, "*.dce-backup-*")
            .Should()
            .ContainSingle()
            .Subject;
        (await File.ReadAllTextAsync(backup)).Should().Be(Original);

        await using var stream = File.OpenRead(path);
        var result = await HtmlExportFile.InspectAsync(
            stream,
            path,
            new Snowflake(42),
            CancellationToken.None
        );
        result.MessageCount.Should().Be(3);
        result.LastMessageId.Should().Be(new Snowflake(3));
    }

    [Fact]
    public async Task Append_supports_older_exports_identified_by_the_filename()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test [42].html");
        var legacy = Original.Replace(" data-channel-id=\"42\"", "", StringComparison.Ordinal);
        await File.WriteAllTextAsync(path, legacy);

        await CreateExporter(new DiscordHandler()).ExportChannelAsync(CreateRequest(path));

        var html = await File.ReadAllTextAsync(path);
        html.Should().Contain("old first").And.Contain("new third");
        var backup = Directory
            .GetFiles(dir.Path, "*.dce-backup-*")
            .Should()
            .ContainSingle()
            .Subject;
        (await File.ReadAllTextAsync(backup)).Should().Be(legacy);
    }

    [Fact]
    public async Task Append_supports_an_export_created_by_the_html_writer()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        var request = CreateRequest(path);
        var context = new ExportContext(new DiscordClient("test-token"), request);
        await using (var writer = new HtmlMessageWriter(File.Create(path), context, "Dark"))
        {
            await writer.WritePreambleAsync();
            using var json = JsonDocument.Parse(
                """
                {"id":"2","type":0,"author":{"id":"99","username":"tester"},"timestamp":"2025-01-01T00:00:00+00:00","content":"original"}
                """
            );
            await writer.WriteMessageAsync(Message.Parse(json.RootElement));
            await writer.WritePostambleAsync();
        }

        await CreateExporter(new DiscordHandler()).ExportChannelAsync(request);

        var html = await File.ReadAllTextAsync(path);
        html.Should().Contain("original").And.Contain("new third");
        html.Should().Contain("Exported 2 message(s)");
    }

    [Fact]
    public async Task A_failed_fetch_does_not_replace_the_original()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);

        var act = async () =>
            await CreateExporter(new DiscordHandler(true)).ExportChannelAsync(CreateRequest(path));

        await act.Should().ThrowAsync<DiscordChatExporterException>();
        (await File.ReadAllTextAsync(path)).Should().Be(Original);
        Directory.GetFiles(dir.Path).Should().ContainSingle();
    }

    [Fact]
    public async Task Append_without_new_messages_leaves_the_original_untouched()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);
        var handler = new DiscordHandler(channelLastId: 2);

        await CreateExporter(handler).ExportChannelAsync(CreateRequest(path, 2));

        (await File.ReadAllTextAsync(path)).Should().Be(Original);
        handler.Requests.Should().Contain("/api/v10/channels/42");
        handler.Requests.Should().NotContain(s => s.Contains("/messages"));
        Directory.GetFiles(dir.Path).Should().ContainSingle();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unavailable_channel_metadata_does_not_replace_the_original(bool failChannel)
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);
        var handler = new DiscordHandler(channelLastId: null, failChannel: failChannel);

        var act = async () => await CreateExporter(handler).ExportChannelAsync(CreateRequest(path));

        await act.Should().ThrowAsync<DiscordChatExporterException>();
        (await File.ReadAllTextAsync(path)).Should().Be(Original);
        Directory.GetFiles(dir.Path).Should().ContainSingle();
    }

    [Fact]
    public async Task An_incomplete_history_does_not_replace_the_original()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);

        var act = async () =>
            await CreateExporter(new DiscordHandler(truncate: true))
                .ExportChannelAsync(CreateRequest(path));

        await act.Should().ThrowAsync<DiscordChatExporterException>();
        (await File.ReadAllTextAsync(path)).Should().Be(Original);
        Directory.GetFiles(dir.Path).Should().ContainSingle();
    }

    [Fact]
    public async Task Cancellation_does_not_replace_the_original()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var act = async () =>
            await CreateExporter(new DiscordHandler())
                .ExportChannelAsync(CreateRequest(path), cancellationToken: cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        (await File.ReadAllTextAsync(path)).Should().Be(Original);
        Directory.GetFiles(dir.Path).Should().ContainSingle();
    }

    [Theory]
    [InlineData("data-channel-id=\"42\"", "data-channel-id=\"43\"")]
    [InlineData("data-channel-id=\"42\"", "data-channel-id=\"42\" data-dce-complete=\"false\"")]
    [InlineData("Exported 2 message(s)", "Exported 3 message(s)")]
    [InlineData("chatlog__message-container-2", "chatlog__message-container-1")]
    public async Task Invalid_exports_are_rejected_without_modification(
        string oldValue,
        string newValue
    )
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        var original = Original.Replace(oldValue, newValue, StringComparison.Ordinal);
        await File.WriteAllTextAsync(path, original);

        var act = async () =>
            await CreateExporter(new DiscordHandler()).ExportChannelAsync(CreateRequest(path));

        await act.Should().ThrowAsync<DiscordChatExporterException>();
        (await File.ReadAllTextAsync(path)).Should().Be(original);
        Directory.GetFiles(dir.Path).Should().ContainSingle();
    }

    [Fact]
    public async Task Partitioned_exports_are_rejected_without_modification()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "test [part 2].html"), "other part");

        var act = async () =>
            await CreateExporter(new DiscordHandler()).ExportChannelAsync(CreateRequest(path));

        await act.Should().ThrowAsync<DiscordChatExporterException>();
        (await File.ReadAllTextAsync(path)).Should().Be(Original);
    }

    [Fact]
    public async Task Append_leaves_the_original_untouched_when_newest_messages_were_deleted()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        await File.WriteAllTextAsync(path, Original);
        var handler = new DiscordHandler(channelLastId: 3, lastExistingMessageId: 2);

        await CreateExporter(handler).ExportChannelAsync(CreateRequest(path));

        (await File.ReadAllTextAsync(path)).Should().Be(Original);
        handler.Requests.Should().Contain(s => s.Contains("limit=1"));
        handler.Requests.Should().NotContain(s => s.Contains("after=2"));
        Directory.GetFiles(dir.Path).Should().ContainSingle();
    }

    [Fact]
    public async Task Inspect_supports_swiss_locale_message_count_separators()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "test.html");
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html><html><body>");
        html.AppendLine("""<div class="chatlog" data-channel-id="42">""");
        for (var i = 1; i <= 1234; i++)
        {
            html.AppendLine(
                $"<div class=chatlog__message-group><div id=chatlog__message-container-{i} data-message-id={i}>msg {i}</div></div>"
            );
        }
        html.AppendLine("</div>");
        html.AppendLine(
            "<div class=postamble><div class=postamble__entry>Exported 1\u2019234 message(s)</div><div class=postamble__entry>Timezone: UTC+0</div></div>"
        );
        await File.WriteAllTextAsync(path, html.ToString());

        await using var stream = File.OpenRead(path);
        var result = await HtmlExportFile.InspectAsync(
            stream,
            path,
            new Snowflake(42),
            CancellationToken.None
        );

        result.MessageCount.Should().Be(1234);
        result.LastMessageId.Should().Be(new Snowflake(1234));
    }

    [Fact]
    public async Task Append_rejects_a_missing_file_path()
    {
        using var dir = TempDirectory.Create();
        var path = Path.Combine(dir.Path, "missing", "test.html");

        var act = async () =>
            await CreateExporter(new DiscordHandler()).ExportChannelAsync(CreateRequest(path));

        await act.Should().ThrowAsync<DiscordChatExporterException>();
    }
}
