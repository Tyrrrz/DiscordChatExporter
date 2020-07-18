using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Exceptions;
using DiscordChatExporter.Domain.Utilities;

namespace DiscordChatExporter.Domain.Exporting
{
    public class ChannelExporter
    {
        private readonly DiscordClient _discord;

        public ChannelExporter(DiscordClient discord) => _discord = discord;

        public ChannelExporter(AuthToken token) : this(new DiscordClient(token)) {}

        public async ValueTask ExportChannelAsync(ExportRequest request, IProgress<double>? progress = null)
        {
            // Build context
            var contextMembers = new HashSet<Member>(IdBasedEqualityComparer.Instance);
            var contextChannels = await _discord.GetGuildChannelsAsync(request.Guild.Id);
            var contextRoles = await _discord.GetGuildRolesAsync(request.Guild.Id);

            var context = new ExportContext(
                request,
                contextMembers,
                contextChannels,
                contextRoles
            );

            // Export messages
            await using var messageExporter = new MessageExporter(context);

            var exportedAnything = false;
            var encounteredUsers = new HashSet<User>(IdBasedEqualityComparer.Instance);
            await foreach (var message in _discord.GetMessagesAsync(request.Channel.Id, request.After, request.Before, progress))
            {
                // Resolve members for referenced users
                foreach (var referencedUser in message.MentionedUsers.Prepend(message.Author))
                {
                    if (!encounteredUsers.Add(referencedUser))
                        continue;

                    var member =
                        await _discord.TryGetGuildMemberAsync(request.Guild.Id, referencedUser) ??
                        Member.CreateForUser(referencedUser);

                    contextMembers.Add(member);
                }

                // Export message
                await messageExporter.ExportMessageAsync(message);
                exportedAnything = true;
            }

            // Throw if no messages were exported
            if (!exportedAnything)
                throw DiscordChatExporterException.ChannelIsEmpty(request.Channel.Name);
        }
    }
}