using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Utils.Extensions;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class ExportMultipleCommandBase : ExportCommandBase
    {
        [CommandOption("parallel", Description = "Limits how many channels can be exported in parallel.")]
        public int ParallelLimit { get; init; } = 1;

        protected async ValueTask ExportChannelsAsync(IConsole console, IReadOnlyList<Channel> channels)
        {
            await console.Output.WriteLineAsync($"Exporting {channels.Count} channel(s)...");

            var errors = new ConcurrentDictionary<Channel, string>();

            await console.CreateProgressTicker().StartAsync(async progressContext =>
            {
                await channels.ParallelForEachAsync(async channel =>
                {
                    try
                    {
                        var guild = await Discord.GetGuildAsync(channel.GuildId);
                        await ExportChannelAsync(guild, channel, progressContext);
                    }
                    catch (DiscordChatExporterException ex) when (!ex.IsCritical)
                    {
                        errors[channel] = ex.Message;
                    }
                }, ParallelLimit.ClampMin(1));

                await console.Output.WriteLineAsync();
            });

            // Print result
            await console.Output.WriteLineAsync(
                $"Successfully exported {channels.Count - errors.Count} channel(s)."
            );

            // Print errors
            if (errors.Any())
            {
                using (console.WithForegroundColor(ConsoleColor.Red))
                    await console.Output.WriteLineAsync($"Failed to export {errors.Count} channel(s):");

                foreach (var (channel, error) in errors)
                {
                    await console.Output.WriteAsync($"{channel.Category} / {channel.Name}: ");

                    using (console.WithForegroundColor(ConsoleColor.Red))
                        await console.Output.WriteLineAsync(error);
                }

                await console.Output.WriteLineAsync();
            }

            // Fail the command if ALL channels failed to export.
            // Having some of the channels fail to export is fine and expected.
            if (errors.Count >= channels.Count)
            {
                throw new CommandException("Export failed.");
            }

            await console.Output.WriteLineAsync("Done.");
        }
    }
}