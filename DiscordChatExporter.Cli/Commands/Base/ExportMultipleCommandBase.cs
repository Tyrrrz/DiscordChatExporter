using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Utilities;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Utils.Extensions;
using Gress;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class ExportMultipleCommandBase : ExportCommandBase
    {
        [CommandOption("parallel", Description = "Limits how many channels can be exported in parallel.")]
        public int ParallelLimit { get; init; } = 1;

        protected async ValueTask ExportMultipleAsync(IConsole console, IReadOnlyList<Channel> channels)
        {
            // This uses a different route from ExportCommandBase.ExportAsync() because it runs
            // in parallel and needs another way to report progress to console.

            await console.Output.WriteAsync(
                $"Exporting {channels.Count} channels... "
            );

            var progress = console.CreateProgressTicker();

            var operations = progress.Wrap().CreateOperations(channels.Count);

            var successfulExportCount = 0;
            var errors = new ConcurrentBag<(Channel, string)>();

            await channels.Zip(operations).ParallelForEachAsync(async tuple =>
            {
                var (channel, operation) = tuple;

                try
                {
                    var guild = await Discord.GetGuildAsync(channel.GuildId);

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

                    await Exporter.ExportChannelAsync(request, operation);

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

            await console.Output.WriteLineAsync();

            foreach (var (channel, error) in errors)
                await console.Error.WriteLineAsync($"Channel '{channel}': {error}");

            await console.Output.WriteLineAsync($"Successfully exported {successfulExportCount} channel(s).");
        }
    }
}