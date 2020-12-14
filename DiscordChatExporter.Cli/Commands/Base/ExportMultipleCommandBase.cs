using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Utilities;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exceptions;
using DiscordChatExporter.Domain.Exporting;
using DiscordChatExporter.Domain.Utilities;
using Gress;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class ExportMultipleCommandBase : ExportCommandBase
    {
        [CommandOption("parallel",
            Description = "Limits how many channels can be exported in parallel.")]
        public int ParallelLimit { get; set; } = 1;

        protected async ValueTask ExportMultipleAsync(IConsole console, IReadOnlyList<Channel> channels)
        {
            // This uses a different route from ExportCommandBase.ExportAsync() because it runs
            // in parallel and needs another way to report progress to console.

            console.Output.Write($"Exporting {channels.Count} channels... ");
            var progress = console.CreateProgressTicker();

            var operations = progress.Wrap().CreateOperations(channels.Count);

            var successfulExportCount = 0;
            var errors = new ConcurrentBag<(Channel, string)>();

            await channels.Zip(operations).ParallelForEachAsync(async tuple =>
            {
                var (channel, operation) = tuple;

                try
                {
                    var guild = await GetDiscordClient().GetGuildAsync(channel.GuildId);

                    var request = new ExportRequest(
                        guild,
                        channel,
                        OutputPath,
                        ExportFormat,
                        After,
                        Before,
                        PartitionLimit,
                        ShouldDownloadMedia,
                        ShouldReuseMedia,
                        DateFormat
                    );

                    await GetChannelExporter().ExportChannelAsync(request, operation);

                    Interlocked.Increment(ref successfulExportCount);
                }
                catch (DiscordChatExporterException ex) when (!ex.IsCritical)
                {
                    errors.Add((channel, ex.Message));
                }
                finally
                {
                    operation.Dispose();
                }
            }, ParallelLimit.ClampMin(1));

            console.Output.WriteLine();

            foreach (var (channel, error) in errors)
                console.Error.WriteLine($"Channel '{channel}': {error}");

            console.Output.WriteLine($"Successfully exported {successfulExportCount} channel(s).");
        }
    }
}