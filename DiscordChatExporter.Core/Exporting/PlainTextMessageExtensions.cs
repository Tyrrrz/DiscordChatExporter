using System.Globalization;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal static class PlainTextMessageExtensions
{
    extension(Message message)
    {
        public string GetFallbackContent() =>
            message.Kind switch
            {
                MessageKind.RecipientAdd => message.MentionedUsers.Any()
                    ? $"Added {message.MentionedUsers.First().DisplayName} to the group."
                    : "Added a recipient.",

                MessageKind.RecipientRemove => message.MentionedUsers.Any()
                    ? message.Author.Id == message.MentionedUsers.First().Id
                        ? "Left the group."
                        : $"Removed {message.MentionedUsers.First().DisplayName} from the group."
                    : "Removed a recipient.",

                MessageKind.Call =>
                    $"Started a call that lasted {message
                            .CallEndedTimestamp?
                            .Pipe(t => t - message.Timestamp)
                            .Pipe(t => t.TotalMinutes)
                            .ToString("n0", CultureInfo.InvariantCulture) ?? "0"} minutes.",

                MessageKind.ChannelNameChange => !string.IsNullOrWhiteSpace(message.Content)
                    ? $"Changed the channel name: {message.Content}"
                    : "Changed the channel name.",

                MessageKind.ChannelIconChange => "Changed the channel icon.",
                MessageKind.ChannelPinnedMessage => "Pinned a message.",
                MessageKind.ThreadCreated => "Started a thread.",
                MessageKind.GuildMemberJoin => "Joined the server.",
                MessageKind.PollResult => message
                    .Embeds.Select(e => e.TryGetPollResult())
                    .WhereNotNull()
                    .FirstOrDefault()
                    is { } pollResult
                    ? string.IsNullOrWhiteSpace(pollResult.Question)
                        ? $"{message.Author.DisplayName}'s poll has closed."
                        : $"{message.Author.DisplayName}'s poll {pollResult.Question} has closed."
                    : "A poll has closed.",

                _ => message.Content,
            };
    }
}
