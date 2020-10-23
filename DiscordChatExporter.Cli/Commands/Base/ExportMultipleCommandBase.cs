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
        [CommandOption("parallel", Description = "Export this number of channels in parallel.")]
        public int ParallelLimit { get; set; } = 1;

        protected async ValueTask ExportMultipleAsync(IConsole console, IReadOnlyList<Channel> channels)
        {
            // HACK: this uses a separate route from ExportCommandBase because the progress ticker is not thread-safe

            console.Output.Write($"Exporting {channels.Count} channels... ");
            var progress = console.CreateProgressTicker();

            var operations = progress.Wrap().CreateOperations(channels.Count);

            var errors = new List<string>();

            var successfulExportCount = 0;
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
                        ExportFormats,
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
                    errors.Add(ex.Message);
                }
                finally
                {
                    operation.Dispose();
                }
            }, ParallelLimit.ClampMin(1));

            console.Output.WriteLine();

            foreach (var error in errors)
                console.Error.WriteLine(error);

            console.Output.WriteLine($"Successfully exported {successfulExportCount} channel(s).");
        }
    }
}