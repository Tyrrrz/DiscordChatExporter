﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using Gress;

namespace DiscordChatExporter.Core.Exporting;

public class ChannelExporter
{
    private readonly DiscordClient _discord;

    public ChannelExporter(DiscordClient discord) => _discord = discord;

    public async ValueTask ExportChannelAsync(
        ExportRequest request,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // Forum channels don't have messages, they are just a list of threads
        if (request.Channel.Kind == ChannelKind.GuildForum)
            throw new DiscordChatExporterException("Channel is a forum.");

        // Check if the channel is empty
        if (request.Channel.IsEmpty)
            throw new DiscordChatExporterException("Channel does not contain any messages.");

        // Check if the 'after' boundary is valid
        if (request.After is not null && !request.Channel.MayHaveMessagesAfter(request.After.Value))
        {
            throw new DiscordChatExporterException(
                "Channel does not contain any messages within the specified period."
            );
        }

        // Check if the 'before' boundary is valid
        if (
            request.Before is not null
            && !request.Channel.MayHaveMessagesBefore(request.Before.Value)
        )
        {
            throw new DiscordChatExporterException(
                "Channel does not contain any messages within the specified period."
            );
        }

        // Build context
        var context = new ExportContext(_discord, request);
        await context.PopulateChannelsAndRolesAsync(cancellationToken);

        // Export messages
        await using var messageExporter = new MessageExporter(context);
        await foreach (
            var message in _discord.GetMessagesAsync(
                request.Channel.Id,
                request.After,
                request.Before,
                progress,
                cancellationToken
            )
        )
        {
            // Resolve members for referenced users
            foreach (var user in message.GetReferencedUsers())
                await context.PopulateMemberAsync(user, cancellationToken);

            // Export the message
            if (request.MessageFilter.IsMatch(message))
                await messageExporter.ExportMessageAsync(message, cancellationToken);
        }

        // Throw if no messages were exported
        if (messageExporter.MessagesExported <= 0)
        {
            throw new DiscordChatExporterException(
                "Channel does not contain any matching messages within the specified period."
            );
        }
    }
}
