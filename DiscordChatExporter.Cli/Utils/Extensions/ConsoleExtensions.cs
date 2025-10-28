using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Logging;
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
                Out = new AnsiConsoleOutput(console.Output),
            }
        );

    public static Status CreateStatusTicker(this IConsole console) =>
        console.CreateAnsiConsole().Status().AutoRefresh(true);

    public static (Progress, ConsoleProgressLogger) CreateProgressTicker(this IConsole console)
    {
        var ansiConsole = console.CreateAnsiConsole();
        var logger = new ConsoleProgressLogger(ansiConsole);
        var progress = ansiConsole
            .Progress()
            .AutoClear(false)
            .AutoRefresh(true)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn { Alignment = Justify.Left },
                new ProgressBarColumn(),
                new PercentageColumn()
            );
        return (progress, logger);
    }

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

/// <summary>
/// The ConsoleProgressLogger is a <see cref="ProgressLogger"/> subclass that logs the status updates of the exported
/// channels with colors on the console.
/// It also provides a way to print the generated export summary on the console.
/// </summary>
/// <param name="console">The console that the progress information and summary is logged on.</param>
public class ConsoleProgressLogger(IAnsiConsole console) : ProgressLogger
{
    /// <inheritdoc/>
    /// <remarks>The ConsoleProgressLogger logs the success message to the console.</remarks>
    public override void LogSuccess(ExportRequest request, string message)
    {
        LogMessage("SUCCESS", "[green]", request, message);
    }

    /// <inheritdoc/>
    /// <remarks>The ConsoleProgressLogger logs the informational message to the console.</remarks>
    public override void LogInfo(ExportRequest request, string message)
    {
        LogMessage("INFO", "[default]", request, message);
    }

    /// <inheritdoc/>
    /// <remarks>The ConsoleProgressLogger logs the warning message to the console.</remarks>
    public override void LogWarning(ExportRequest request, string message)
    {
        LogMessage("WARNING", "[yellow]", request, message);
    }

    /// <inheritdoc/>
    /// <remarks>The ConsoleProgressLogger logs the error message to the console.</remarks>
    public override void LogError(ExportRequest? request, string message)
    {
        IncrementCounter(ExportResult.ExportError);
        LogMessage("ERROR", "[red]", request, message);
    }

    /// <summary>
    /// Logs the given message of the given category about the current channel export with the given color to the
    /// console.
    /// </summary>
    /// <param name="category">The category of the message that should be logged.</param>
    /// <param name="color">The color in which the message should be logged.</param>
    /// <param name="request">The request specifying the current channel export.</param>
    /// <param name="message">The message about the current channel export that should be logged.</param>
    private void LogMessage(string category, string color, ExportRequest? request, string message)
    {
        var paddedCategory = (category + ":").PadRight(10);

        var channelInfo = "";
        if (request != null)
            channelInfo =
                request.Guild.Name + " / " + request.Channel.GetHierarchicalName() + " | ";

        var logMessage = $"{color}{paddedCategory}{channelInfo}{message}[/]";
        console.MarkupLine(logMessage);
    }

    /// <summary>
    /// Prints a summary on all previously logged exports and their respective results to the console.
    /// </summary>
    /// <param name="updateType">The file exists handling of the export whose summary should be printed.</param>
    public void PrintExportSummary(FileExistsHandling updateType)
    {
        var exportSummary = GetExportSummary(updateType);
        exportSummary.TryGetValue(ExportResult.NewExportSuccess, out var newExportSuccessMessage);
        exportSummary.TryGetValue(
            ExportResult.NewExportSuccessEmpty,
            out var newExportSuccessEmptyMessage
        );
        exportSummary.TryGetValue(
            ExportResult.UpdateExportSuccess,
            out var updateExportSuccessMessage
        );
        exportSummary.TryGetValue(ExportResult.UpdateExportSkip, out var updateExportSkipMessage);
        exportSummary.TryGetValue(ExportResult.ExportError, out var exportErrorMessage);

        if (newExportSuccessMessage != null)
            console.MarkupLine($"[green]{newExportSuccessMessage}[/]");
        if (newExportSuccessEmptyMessage != null)
            console.MarkupLine($"[default]{newExportSuccessEmptyMessage}[/]");
        if (updateExportSuccessMessage != null)
            console.MarkupLine($"[green]{updateExportSuccessMessage}[/]");
        if (updateExportSkipMessage != null)
            console.MarkupLine($"[default]{updateExportSkipMessage}[/]");
        if (exportErrorMessage != null)
            console.MarkupLine($"[red]{exportErrorMessage}[/]");
    }
}
