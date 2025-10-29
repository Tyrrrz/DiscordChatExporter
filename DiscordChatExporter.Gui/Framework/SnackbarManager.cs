using System;
using Avalonia.Threading;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Logging;
using Material.Styles.Controls;
using Material.Styles.Models;

namespace DiscordChatExporter.Gui.Framework;

public class SnackbarManager
{
    private readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(5);

    public void Notify(string message, TimeSpan? duration = null) =>
        SnackbarHost.Post(
            new SnackbarModel(message, duration ?? _defaultDuration),
            null,
            DispatcherPriority.Normal
        );

    public void Notify(
        string message,
        string actionText,
        Action actionHandler,
        TimeSpan? duration = null
    ) =>
        SnackbarHost.Post(
            new SnackbarModel(
                message,
                duration ?? _defaultDuration,
                new SnackbarButtonModel { Text = actionText, Action = actionHandler }
            ),
            null,
            DispatcherPriority.Normal
        );
}

/// <summary>
/// The SnackbarProgressLogger is a <see cref="ProgressLogger"/> subclass that logs the status updates of the exported
/// channels in the GUI snackbar.
/// </summary>
/// <param name="snackbarManager">
/// The snackbar manager that's used to control the snackbar and add log messages to it.
/// </param>
public class SnackbarProgressLogger(SnackbarManager snackbarManager) : ProgressLogger
{
    /// <inheritdoc/>
    /// <remarks>The SnackbarProgressLogger logs the success message in the GUI snackbar.</remarks>
    public override void LogSuccess(ExportRequest request, string message)
    {
        LogMessage("SUCCESS", request, message);
    }

    /// <inheritdoc/>
    /// <remarks>The SnackbarProgressLogger logs the informational message in the GUI snackbar.</remarks>
    public override void LogInfo(ExportRequest request, string message)
    {
        LogMessage("INFO", request, message);
    }

    /// <inheritdoc/>
    /// <remarks>The SnackbarProgressLogger logs the warning message in the GUI snackbar.</remarks>
    public override void LogWarning(ExportRequest request, string message)
    {
        LogMessage("WARNING", request, message);
    }

    /// <inheritdoc/>
    /// <remarks>The SnackbarProgressLogger logs the error message in the GUI snackbar.</remarks>
    public override void LogError(ExportRequest? request, string message)
    {
        IncrementCounter(ExportResult.ExportError);
        // TODO: Maybe highlight error messages with a red background
        LogMessage("ERROR", request, message, TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Logs the given message of the given category about the current channel export in the GUI snackbar.
    /// </summary>
    /// <param name="category">The category of the message that should be logged.</param>
    /// <param name="request">The request specifying the current channel export.</param>
    /// <param name="message">The message about the current channel export that should be logged.</param>
    /// <param name="duration">
    /// The duration the message should be displayed in the snackbar.
    /// If the given value is null, the default duration is used.
    /// </param>
    private void LogMessage(
        string category,
        ExportRequest? request,
        string message,
        TimeSpan? duration = null
    )
    {
        var channelInfo = "";
        if (request != null)
            channelInfo =
                request.Guild.Name + " / " + request.Channel.GetHierarchicalName() + " | ";

        var logMessage = $"{category}: {channelInfo}{message}";
        snackbarManager.Notify(logMessage, duration);
    }

    /// <summary>
    /// Prints a summary on all previously logged exports and their respective results in the GUI snackbar.
    /// </summary>
    /// <param name="updateType">The file exists handling of the export whose summary should be printed.</param>
    public void PrintExportSummary(ExportExistsHandling updateType)
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

        var summaryString = "";
        if (newExportSuccessMessage != null)
            summaryString += newExportSuccessMessage + "\n";
        if (newExportSuccessEmptyMessage != null)
            summaryString += newExportSuccessEmptyMessage + "\n";
        if (updateExportSuccessMessage != null)
            summaryString += updateExportSuccessMessage + "\n";
        if (updateExportSkipMessage != null)
            summaryString += updateExportSkipMessage + "\n";
        if (exportErrorMessage != null)
            summaryString += exportErrorMessage + "\n";

        snackbarManager.Notify(summaryString.TrimEnd(), TimeSpan.FromSeconds(15));
    }
}
