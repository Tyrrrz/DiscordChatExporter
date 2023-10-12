using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Utils.Extensions;

internal static class ConsoleExtensions
{
    public static IAnsiConsole CreateAnsiConsole(this IConsole console) =>
        AnsiConsole.Create(
            new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect,
                Out = new AnsiConsoleOutput(console.Output)
            }
        );

    public static Status CreateStatusTicker(this IConsole console) =>
        console.CreateAnsiConsole().Status().AutoRefresh(true);

    public static Progress CreateProgressTicker(this IConsole console) =>
        console
            .CreateAnsiConsole()
            .Progress()
            .AutoClear(false)
            .AutoRefresh(true)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn { Alignment = Justify.Left },
                new ProgressBarColumn(),
                new PercentageColumn()
            );

    public static async ValueTask StartTaskAsync(
        this ProgressContext context,
        string description,
        Func<ProgressTask, ValueTask> performOperationAsync
    )
    {
        // Description cannot be empty
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/1133
        var actualDescription = !string.IsNullOrWhiteSpace(description) ? description : "...";

        var progressTask = context.AddTask(
            actualDescription,
            new ProgressTaskSettings { MaxValue = 1 }
        );

        try
        {
            await performOperationAsync(progressTask);
        }
        finally
        {
            progressTask.Value = progressTask.MaxValue;
            progressTask.StopTask();
        }
    }
}
