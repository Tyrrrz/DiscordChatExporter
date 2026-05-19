using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exceptions;
using PowerKit.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("deletemessages", Description = "Deletes messages from a channel.")]
public partial class DeleteMessagesCommand : DiscordCommandBase
{
    [CommandOption(
        "channel",
        'c',
        Description = "Channel ID. Note: You can only delete your own messages."
    )]
    public required Snowflake ChannelId { get; set; }

    [CommandOption(
        "before",
        Description = "Limit to messages sent before this date (formatted using the current culture)."
    )]
    public DateTimeOffset? Before { get; set; }

    [CommandOption(
        "after",
        Description = "Limit to messages sent after this date (formatted using the current culture)."
    )]
    public DateTimeOffset? After { get; set; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        // Get current user
        await console.Output.WriteLineAsync("Getting current user...");
        var currentUser = await Discord.GetCurrentUserAsync(cancellationToken);
        await console.Output.WriteLineAsync($"Authenticated as: {currentUser.FullName}");
        await console.Output.WriteLineAsync();

        // Resolve the channel
        await console.Output.WriteLineAsync("Resolving channel...");
        var channel = await Discord.GetChannelAsync(ChannelId, cancellationToken);

        // Warning message
        using (console.WithForegroundColor(ConsoleColor.Yellow))
        {
            await console.Output.WriteLineAsync(
                "WARNING: This will delete your messages from the channel."
            );
            await console.Output.WriteLineAsync("Messages from other users will be skipped.");
        }

        await console.Output.WriteLineAsync();
        await console.Output.WriteLineAsync($"Channel: {channel.Name}");

        if (After is not null)
            await console.Output.WriteLineAsync($"After: {After:g}");

        if (Before is not null)
            await console.Output.WriteLineAsync($"Before: {Before:g}");

        await console.Output.WriteLineAsync();

        // Count user's messages
        await console.Output.WriteLineAsync("Counting your messages...");

        var beforeSnowflake = Before?.Pipe(Snowflake.FromDate);
        var afterSnowflake = After?.Pipe(Snowflake.FromDate);

        var userMessageIds = new List<Snowflake>();
        await foreach (
            var message in Discord.GetMessagesAsync(
                ChannelId,
                afterSnowflake,
                beforeSnowflake,
                null,
                cancellationToken
            )
        )
        {
            if (message.Author.Id == currentUser.Id)
            {
                userMessageIds.Add(message.Id);
            }
        }

        var totalUserMessages = userMessageIds.Count;
        await console.Output.WriteLineAsync($"Found {totalUserMessages} of your messages");
        await console.Output.WriteLineAsync();

        if (totalUserMessages == 0)
        {
            using (console.WithForegroundColor(ConsoleColor.Yellow))
            {
                await console.Output.WriteLineAsync("No messages to delete.");
            }
            return;
        }

        // Delete messages
        await console.Output.WriteLineAsync("Deleting messages...");

        var successCount = 0;
        var failedCount = 0;

        foreach (var messageId in userMessageIds)
        {
            try
            {
                var deleted = await Discord.DeleteMessageAsync(
                    ChannelId,
                    messageId,
                    cancellationToken
                );

                if (deleted)
                {
                    successCount++;
                    using (console.WithForegroundColor(ConsoleColor.Green))
                    {
                        await console.Output.WriteAsync("\r");
                        await console.Output.WriteAsync(
                            $"✓ Deleted {successCount} / {totalUserMessages} messages          "
                        );
                    }
                }
                else
                {
                    failedCount++;
                }

                // Discord's rate limit headers are handled automatically by DeleteMessageAsync
            }
            catch (DiscordChatExporterException ex) when (!ex.IsFatal)
            {
                failedCount++;
                using (console.WithForegroundColor(ConsoleColor.Red))
                {
                    await console.Output.WriteAsync("\r");
                    await console.Output.WriteAsync(
                        $"✗ Error | Deleted: {successCount} / {totalUserMessages}, Failed: {failedCount}          "
                    );
                }
            }
        }

        // Clear the progress line
        await console.Output.WriteLineAsync();
        await console.Output.WriteLineAsync();

        // Report results
        await console.Output.WriteLineAsync("Deletion complete!");

        using (console.WithForegroundColor(ConsoleColor.Green))
        {
            await console.Output.WriteLineAsync(
                $"Successfully deleted: {successCount} / {totalUserMessages} message(s)"
            );
        }

        if (failedCount > 0)
        {
            using (console.WithForegroundColor(ConsoleColor.Red))
            {
                await console.Output.WriteLineAsync($"Failed to delete: {failedCount} message(s)");
            }
        }
    }
}
