using CliFx.Infrastructure;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Utils.Extensions
{
    internal static class ConsoleExtensions
    {
        public static IAnsiConsole CreateAnsiConsole(this IConsole console) => AnsiConsole.Create(
            new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect,
                Out = console.Output
            }
        );

        public static Progress CreateProgressTicker(this IConsole console) => console
            .CreateAnsiConsole()
            .Progress()
            .AutoClear(false)
            .AutoRefresh(true)
            .HideCompleted(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn {Alignment = Justify.Left},
                new ProgressBarColumn(),
                new PercentageColumn()
            });
    }
}