using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Utils.Extensions;
using Gress;

namespace DiscordChatExporter.Core.Exporting;

public class ChannelExporter
{
    private readonly DiscordClient _discord;

    public ChannelExporter(DiscordClient discord) => _discord = discord;

    public async ValueTask ExportChannelAsync(
        ExportRequest request,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Build context
        var contextMembers = new Dictionary<Snowflake, Member>();

        var contextChannels = (await _discord.GetGuildChannelsAsync(request.Guild.Id, cancellationToken))
            .ToDictionary(c => c.Id);

        var contextRoles = (await _discord.GetGuildRolesAsync(request.Guild.Id, cancellationToken))
            .ToDictionary(r => r.Id);

        var context = new ExportContext(
            _discord,
            request,
            contextMembers,
            contextChannels,
            contextRoles
        );

        // Export messages
        await using var messageExporter = new MessageExporter(context);

        await foreach (var message in _discord.GetMessagesAsync(
                           request.Channel.Id,
                           request.After,
                           request.Before,
                           progress,
                           cancellationToken))
        {
            // Resolve members for referenced users
            foreach (var referencedUser in message.MentionedUsers.Prepend(message.Author))
            {
                if (contextMembers.ContainsKey(referencedUser.Id))
                    continue;

                var member = await _discord.GetGuildMemberAsync(
                    request.Guild.Id,
                    referencedUser,
                    cancellationToken
                );

                contextMembers[member.Id] = member;
            }

            // Export the message
            if (request.MessageFilter.IsMatch(message))
                await messageExporter.ExportMessageAsync(message, cancellationToken);
        }

        // Throw if no messages were exported
        if (messageExporter.MessagesExported <= 0)
            throw DiscordChatExporterException.ChannelIsEmpty();
    }
}