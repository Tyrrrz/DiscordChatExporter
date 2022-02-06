﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;

namespace DiscordChatExporter.Core.Exporting.Writers;

internal class PlainTextMessageWriter : MessageWriter
{
    private readonly TextWriter _writer;

    public PlainTextMessageWriter(Stream stream, ExportContext context)
        : base(stream, context)
    {
        _writer = new StreamWriter(stream);
    }

    private string FormatMarkdown(string? markdown) =>
        PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

    private async ValueTask WriteMessageHeaderAsync(Message message)
    {
        // Timestamp & author
        await _writer.WriteAsync($"[{Context.FormatDate(message.Timestamp)}]");
        await _writer.WriteAsync($" {message.Author.FullName}");

        // Whether the message is pinned
        if (message.IsPinned)
            await _writer.WriteAsync(" (pinned)");

        await _writer.WriteLineAsync();
    }

    private async ValueTask WriteAttachmentsAsync(
        IReadOnlyList<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        if (!attachments.Any())
            return;

        await _writer.WriteLineAsync("{Attachments}");

        foreach (var attachment in attachments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _writer.WriteLineAsync(await Context.ResolveMediaUrlAsync(attachment.Url, cancellationToken));
        }

        await _writer.WriteLineAsync();
    }

    private async ValueTask WriteEmbedsAsync(
        IReadOnlyList<Embed> embeds,
        CancellationToken cancellationToken = default)
    {
        foreach (var embed in embeds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _writer.WriteLineAsync("{Embed}");

            if (!string.IsNullOrWhiteSpace(embed.Author?.Name))
                await _writer.WriteLineAsync(embed.Author.Name);

            if (!string.IsNullOrWhiteSpace(embed.Url))
                await _writer.WriteLineAsync(embed.Url);

            if (!string.IsNullOrWhiteSpace(embed.Title))
                await _writer.WriteLineAsync(FormatMarkdown(embed.Title));

            if (!string.IsNullOrWhiteSpace(embed.Description))
                await _writer.WriteLineAsync(FormatMarkdown(embed.Description));

            foreach (var field in embed.Fields)
            {
                if (!string.IsNullOrWhiteSpace(field.Name))
                    await _writer.WriteLineAsync(FormatMarkdown(field.Name));

                if (!string.IsNullOrWhiteSpace(field.Value))
                    await _writer.WriteLineAsync(FormatMarkdown(field.Value));
            }

            if (!string.IsNullOrWhiteSpace(embed.Thumbnail?.Url))
                await _writer.WriteLineAsync(await Context.ResolveMediaUrlAsync(embed.Thumbnail.ProxyUrl ?? embed.Thumbnail.Url, cancellationToken));

            if (!string.IsNullOrWhiteSpace(embed.Image?.Url))
                await _writer.WriteLineAsync(await Context.ResolveMediaUrlAsync(embed.Image.ProxyUrl ?? embed.Image.Url, cancellationToken));

            if (!string.IsNullOrWhiteSpace(embed.Footer?.Text))
                await _writer.WriteLineAsync(embed.Footer.Text);

            await _writer.WriteLineAsync();
        }
    }

    private async ValueTask WriteStickersAsync(
        IReadOnlyList<Sticker> stickers,
        CancellationToken cancellationToken = default)
    {
        if (!stickers.Any())
            return;

        await _writer.WriteLineAsync("{Stickers}");

        foreach (var sticker in stickers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _writer.WriteLineAsync(await Context.ResolveMediaUrlAsync(sticker.SourceUrl, cancellationToken));
        }

        await _writer.WriteLineAsync();
    }

    private async ValueTask WriteReactionsAsync(
        IReadOnlyList<Reaction> reactions,
        CancellationToken cancellationToken = default)
    {
        if (!reactions.Any())
            return;

        await _writer.WriteLineAsync("{Reactions}");

        foreach (var reaction in reactions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _writer.WriteAsync(reaction.Emoji.Name);

            if (reaction.Count > 1)
                await _writer.WriteAsync($" ({reaction.Count})");

            await _writer.WriteAsync(' ');
        }

        await _writer.WriteLineAsync();
    }

    public override async ValueTask WritePreambleAsync(CancellationToken cancellationToken = default)
    {
        await _writer.WriteLineAsync(new string('=', 62));
        await _writer.WriteLineAsync($"Guild: {Context.Request.Guild.Name}");
        await _writer.WriteLineAsync($"Channel: {Context.Request.Channel.Category.Name} / {Context.Request.Channel.Name}");

        if (!string.IsNullOrWhiteSpace(Context.Request.Channel.Topic))
            await _writer.WriteLineAsync($"Topic: {Context.Request.Channel.Topic}");

        if (Context.Request.After is not null)
            await _writer.WriteLineAsync($"After: {Context.FormatDate(Context.Request.After.Value.ToDate())}");

        if (Context.Request.Before is not null)
            await _writer.WriteLineAsync($"Before: {Context.FormatDate(Context.Request.Before.Value.ToDate())}");

        await _writer.WriteLineAsync(new string('=', 62));
        await _writer.WriteLineAsync();
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        await base.WriteMessageAsync(message, cancellationToken);

        // Header
        await WriteMessageHeaderAsync(message);

        // Content
        if (!string.IsNullOrWhiteSpace(message.Content))
            await _writer.WriteLineAsync(FormatMarkdown(message.Content));

        await _writer.WriteLineAsync();

        // Attachments, embeds, reactions, etc.
        await WriteAttachmentsAsync(message.Attachments, cancellationToken);
        await WriteEmbedsAsync(message.Embeds, cancellationToken);
        await WriteStickersAsync(message.Stickers, cancellationToken);
        await WriteReactionsAsync(message.Reactions, cancellationToken);

        await _writer.WriteLineAsync();
    }

    public override async ValueTask WritePostambleAsync(CancellationToken cancellationToken = default)
    {
        await _writer.WriteLineAsync(new string('=', 62));
        await _writer.WriteLineAsync($"Exported {MessagesWritten:N0} message(s)");
        await _writer.WriteLineAsync(new string('=', 62));
    }

    public override async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
        await base.DisposeAsync();
    }
}