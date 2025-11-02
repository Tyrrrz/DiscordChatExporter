using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Exporting.Logging;

/// <summary>
/// The ProgressLogger provides a consistent way to log the entire progress of an export, meaning all status updates of
/// each of the exported channels.
/// It also allows to count the different results of the individual channel exports and can provide a summary of them.
/// </summary>
public abstract class ProgressLogger
{
    private readonly ConcurrentDictionary<ExportResult, int> _counters = [];

    /// <summary>
    /// Increments the internal counter of the given export result in a thread-safe way.
    /// </summary>
    /// <param name="exportResult">The export result whose counter should be incremented.</param>
    public void IncrementCounter(ExportResult exportResult)
    {
        _counters.AddOrUpdate(exportResult, 1, (_, currentCount) => currentCount + 1);
    }

    /// <summary>
    /// Generates and returns a summary on all previously logged exports and their respective results.
    /// The summary is returned as one string for each export result that occurred at least once.
    /// </summary>
    /// <param name="updateType">The export exists handling of the export whose summary should be returned.</param>
    /// <returns>A summary on all previously logged exports and their respective results.</returns>
    protected Dictionary<ExportResult, string> GetExportSummary(ExportExistsHandling updateType)
    {
        _counters.TryGetValue(ExportResult.NewExportSuccess, out var newExportSuccessCount);
        _counters.TryGetValue(
            ExportResult.NewExportSuccessEmpty,
            out var newExportSuccessEmptyCount
        );
        _counters.TryGetValue(ExportResult.UpdateExportSuccess, out var updateExportSuccessCount);
        _counters.TryGetValue(ExportResult.UpdateExportSkip, out var updateExportSkipCount);
        _counters.TryGetValue(ExportResult.ExportError, out var exportErrorCount);

        Dictionary<ExportResult, string> exportSummary = [];
        if (newExportSuccessCount > 0)
            exportSummary[ExportResult.NewExportSuccess] =
                $"Successfully exported {newExportSuccessCount} new channel(s).";
        if (newExportSuccessEmptyCount > 0)
            exportSummary[ExportResult.NewExportSuccessEmpty] =
                $"{newExportSuccessEmptyCount} of those channel(s) has / have been empty.";
        if (updateExportSuccessCount > 0)
            exportSummary[ExportResult.UpdateExportSuccess] =
                "Successfully "
                + (updateType == ExportExistsHandling.Append ? "appended" : "overrode")
                + $" {updateExportSuccessCount} existing channel export(s).";
        if (updateExportSkipCount > 0)
            exportSummary[ExportResult.UpdateExportSkip] =
                $"Skipped {updateExportSkipCount} existing up-to-date channel export(s).";
        if (exportErrorCount > 0)
            exportSummary[ExportResult.ExportError] =
                $"Failed to export {exportErrorCount} channel(s) (see prior error message(s)).";

        return exportSummary;
    }

    /// <summary>
    /// Returns whether all exports have failed.
    /// If exports have been skipped as there are no new messages to append, this still returns true if no export
    /// finished successfully and at least one failed.
    /// </summary>
    /// <returns>Whether all exports have failed.</returns>
    public bool AllFailed()
    {
        _counters.TryGetValue(ExportResult.NewExportSuccess, out var newExportSuccessCount);
        _counters.TryGetValue(ExportResult.UpdateExportSuccess, out var updateExportSuccessCount);
        _counters.TryGetValue(ExportResult.ExportError, out var exportErrorCount);
        return newExportSuccessCount + updateExportSuccessCount == 0 && exportErrorCount > 0;
    }

    /// <summary>
    /// Logs the success of the current channel export.
    /// </summary>
    /// <param name="request">The request specifying the current channel export</param>
    /// <param name="message">The success message about the current channel export that should be logged.</param>
    public abstract void LogSuccess(ExportRequest request, string message);

    /// <summary>
    /// Logs an informational message about the current channel export.
    /// </summary>
    /// <param name="request">The request specifying the current channel export.</param>
    /// <param name="message">The informational message about the current channel export that should be logged.</param>
    public abstract void LogInfo(ExportRequest request, string message);

    /// <summary>
    /// Logs a warning message about the current channel export.
    /// </summary>
    /// <param name="request">The request specifying the current channel export.</param>
    /// <param name="message">The warning message about the current channel export that should be logged.</param>
    public abstract void LogWarning(ExportRequest request, string message);

    /// <summary>
    /// Logs an error message about the current channel export.
    /// If this is called, the count of channels that failed is automatically increased.
    /// </summary>
    /// <param name="request">The request specifying the current channel export, if it can be accessed.</param>
    /// <param name="message">The error message about the current channel export that should be logged.</param>
    public abstract void LogError(ExportRequest? request, string message);
}
