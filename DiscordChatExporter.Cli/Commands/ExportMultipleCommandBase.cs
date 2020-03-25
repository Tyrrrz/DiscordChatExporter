using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Utilities;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Models.Exceptions;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Core.Services.Exceptions;
using Gress;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Commands
{
    public abstract class ExportMultipleCommandBase : ExportCommandBase
    {
        [CommandOption("parallel", Description = "Export this number of separate channels in parallel.")]
        public int ParallelLimit { get; set; } = 1;

        protected ExportMultipleCommandBase(SettingsService settingsService, DataService dataService, ExportService exportService)
            : base(settingsService, dataService, exportService)
        {
        }

        protected async ValueTask ExportMultipleAsync(IConsole console, IReadOnlyList<Channel> channels)
        {
            // This uses a separate route from ExportCommandBase because the progress ticker is not thread-safe
            // Ugly code ahead. Will need to refactor.

            if (!string.IsNullOrWhiteSpace(DateFormat))
                SettingsService.DateFormat = DateFormat;

            // Progress
            console.Output.Write($"Exporting {channels.Count} channels... ");
            var ticker = console.CreateProgressTicker();

            // TODO: refactor this after improving Gress
            var progressManager = new ProgressManager();
            progressManager.PropertyChanged += (sender, args) => ticker.Report(progressManager.Progress);

            var operations = progressManager.CreateOperations(channels.Count);

            // Export channels
            using var semaphore = new SemaphoreSlim(ParallelLimit.ClampMin(1));

            var errors = new List<string>();

            await Task.WhenAll(channels.Select(async (channel, i) =>
            {
                var operation = operations[i];
                await semaphore.WaitAsync();

                var guild = await DataService.GetGuildAsync(Token, channel.GuildId);

                try
                {
                    await ExportService.ExportChatLogAsync(Token, guild, channel,
                        OutputPath, ExportFormat, PartitionLimit,
                        After, Before, operation);
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    errors.Add("You don't have access to this channel.");
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    errors.Add("This channel doesn't exist.");
                }
                catch (DomainException ex)
                {
                    errors.Add(ex.Message);
                }
                finally
                {
                    semaphore.Release();
                    operation.Dispose();
                }
            }));

            ticker.Report(1);
            console.Output.WriteLine();

            foreach (var error in errors)
                console.Error.WriteLine(error);

            console.Output.WriteLine("Done.");
        }
    }
}