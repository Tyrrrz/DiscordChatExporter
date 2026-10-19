using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Utils.Logging;
using Gress;

namespace DiscordChatExporter.Core.Exporting;

public class ChannelExporter(DiscordClient discord)
{
    public async ValueTask ExportChannelAsync(
        ExportRequest request,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var _ = Logger.BeginScope($"ch:{request.Channel.Id}");
        var startTimestamp = Stopwatch.GetTimestamp();

        Logger.Info(
            LogCategory.Export,
            $"Exporting channel '{request.Channel.GetHierarchicalName()}' (#{request.Channel.Id}) in guild '{request.Guild.Name}' (#{request.Guild.Id}) -> '{request.OutputFilePath}' [Format: {request.Format}, Media: {request.ShouldDownloadAssets}]."
        );

        // Forum channels don't have messages, they are just a list of threads
        if (request.Channel.Kind == ChannelKind.GuildForum)
        {
            Logger.Error(
                LogCategory.Export,
                $"Channel '{request.Channel.Name}' (#{request.Channel.Id}) of guild '{request.Guild.Name}' (#{request.Guild.Id}) is a forum and cannot be exported directly."
            );

            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"is a forum and cannot be exported directly. "
                    + "You need to pull its threads and export them individually."
            );
        }

        // Build context
        var context = new ExportContext(discord, request);
        await context.PopulateChannelsAndRolesAsync(cancellationToken);

        // Initialize the exporter before further checks to ensure the file is created even if
        // an exception is thrown after this point.
        await using var messageExporter = new MessageExporter(context);

        void LogCompletion()
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            Logger.Info(
                LogCategory.Export,
                $"Finished exporting '{request.Channel.GetHierarchicalName()}' (#{request.Channel.Id}): {messageExporter.MessagesExported} message(s), {messageExporter.PartitionsCreated} partition(s) in {elapsed.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture)} (Members: {context.MemberCacheHits} cached / {context.MembersFetched} fetched / {context.MemberFallbackLookups} fallback; Assets: {context.ReusedAssetsCount} reused / {context.DownloadedAssetsCount} downloaded / {context.FailedAssetsCount} failed)."
            );
        }

        // Check if the channel is empty
        if (request.Channel.IsEmpty)
        {
            Logger.Warning(
                LogCategory.Export,
                $"Channel '{request.Channel.Name}' (#{request.Channel.Id}) of guild '{request.Guild.Name}' does not contain any messages; an empty file will be created."
            );

            LogCompletion();

            throw new ChannelEmptyException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages; an empty file will be created."
            );
        }

        // Check if the 'before' and 'after' boundaries are valid
        if (
            (
                request.Before is not null
                && !request.Channel.MayHaveMessagesBefore(request.Before.Value)
            )
            || (
                request.After is not null
                && !request.Channel.MayHaveMessagesAfter(request.After.Value)
            )
        )
        {
            Logger.Warning(
                LogCategory.Export,
                $"Channel '{request.Channel.Name}' (#{request.Channel.Id}) of guild '{request.Guild.Name}' does not contain any messages within the specified period; an empty file will be created."
            );

            LogCompletion();

            throw new ChannelEmptyException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages within the specified period; an empty file will be created."
            );
        }

        var messages = !request.IsReverseMessageOrder
            ? discord.GetMessagesAsync(
                request.Channel.Id,
                request.After,
                request.Before,
                progress,
                cancellationToken
            )
            : discord.GetMessagesInReverseAsync(
                request.Channel.Id,
                request.After,
                request.Before,
                progress,
                cancellationToken
            );

        await foreach (var message in messages)
        {
            try
            {
                // Resolve members for referenced users
                foreach (var user in message.GetReferencedUsers())
                    await context.PopulateMemberAsync(user, cancellationToken);

                // Export the message
                if (request.MessageFilter.IsMatch(message))
                    await messageExporter.ExportMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    LogCategory.Export,
                    $"Failed to export message #{message.Id} in channel '{request.Channel.Name}' (#{request.Channel.Id}) of guild '{request.Guild.Name}' (#{request.Guild.Id}).",
                    ex
                );

                // Provide more context to the exception, to simplify debugging based on error messages
                throw new DiscordChatExporterException(
                    $"Failed to export message #{message.Id} "
                        + $"in channel '{request.Channel.Name}' (#{request.Channel.Id}) "
                        + $"of guild '{request.Guild.Name} (#{request.Guild.Id})'.",
                    ex is not DiscordChatExporterException dex || dex.IsFatal,
                    ex
                );
            }
        }

        LogCompletion();
    }
}
