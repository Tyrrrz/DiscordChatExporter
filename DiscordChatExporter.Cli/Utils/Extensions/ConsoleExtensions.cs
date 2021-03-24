using CliFx.Infrastructure;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Utils.Extensions
{
    internal static class ConsoleExtensions
    {
        public static IAnsiConsole CreateAnsiConsole(this IConsole console)
        {
            var ansiConsole = AnsiConsole.Create(
                new AnsiConsoleSettings
                {
                    Ansi = AnsiSupport.Detect,
                    ColorSystem = ColorSystemSupport.Detect,
                    Out = console.Output
                }
            );

            // HACK: https://github.com/spectresystems/spectre.console/pull/318
            ansiConsole.Profile.Encoding = console.Output.Encoding;

            return ansiConsole;
        }

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