using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Utils.Extensions
{
    internal static class ConsoleExtensions
    {
        public static IAnsiConsole CreateAnsiConsole(this IConsole console) =>
            AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect,
                Out = new AnsiConsoleOutput(console.Output)
            });

        public static Progress CreateProgressTicker(this IConsole console) => console
            .CreateAnsiConsole()
            .Progress()
            .AutoClear(false)
            .AutoRefresh(true)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn {Alignment = Justify.Left},
                new ProgressBarColumn(),
                new PercentageColumn()
            );

        public static async ValueTask StartTaskAsync(
            this ProgressContext progressContext,
            string description,
            Func<ProgressTask, ValueTask> performOperationAsync)
        {
            var progressTask = progressContext.AddTask(
                // Don't recognize random square brackets as style tags
                Markup.Escape(description),
                new ProgressTaskSettings {MaxValue = 1}
            );

            try
            {
                await performOperationAsync(progressTask);
            }
            finally
            {
                progressTask.StopTask();
            }
        }
    }
}