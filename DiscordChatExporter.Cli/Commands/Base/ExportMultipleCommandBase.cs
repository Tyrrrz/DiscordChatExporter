using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Utilities;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exceptions;
using DiscordChatExporter.Domain.Utilities;
using Gress;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class ExportMultipleCommandBase : ExportCommandBase
    {
        [CommandOption("parallel", Description = "Export this number of separate channels in parallel.")]
        public int ParallelLimit { get; set; } = 1;

        protected async ValueTask ExportMultipleAsync(IConsole console, IReadOnlyList<Channel> channels)
        {
            // This uses a separate route from ExportCommandBase because the progress ticker is not thread-safe
            // Ugly code ahead. Will need to refactor.

            // Progress
            console.Output.Write($"Exporting {channels.Count} channels... ");
            var ticker = console.CreateProgressTicker();

            // TODO: refactor this after improving Gress
            var progressManager = new ProgressManager();
            progressManager.PropertyChanged += (sender, args) => ticker.Report(progressManager.Progress);

            var operations = progressManager.CreateOperations(channels.Count);

            // Export channels

            var errors = new List<string>();

            var successfulExportCount = 0;
            await channels.Zip(operations).ParallelForEachAsync(async tuple =>
            {
                var (channel, operation) = tuple;

                try
                {
                    var guild = await GetDiscordClient().GetGuildAsync(channel.GuildId);

                    await GetExporter().ExportChatLogAsync(guild, channel,
                        OutputPath, ExportFormat, DateFormat, PartitionLimit,
                        After, Before, operation);

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

            ticker.Report(1);
            console.Output.WriteLine();

            foreach (var error in errors)
                console.Error.WriteLine(error);

            console.Output.WriteLine($"Successfully exported {successfulExportCount} channel(s).");
        }
    }
}