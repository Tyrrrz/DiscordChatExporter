namespace DiscordChatExporter.Core.Exporting.Logging;

/// <summary>
/// Represents a possible result of the export of a single channel
/// </summary>
public enum ExportResult
{
    /// <summary>
    /// The channel is a new channel that has been exported successfully.
    /// </summary>
    NewExportSuccess,

    /// <summary>
    /// The channel is a new empty channel that has been exported successfully.
    /// </summary>
    NewExportSuccessEmpty,

    /// <summary>
    /// The channel is a channel that had already been exported and has been appended or overwritten successfully.
    /// </summary>
    UpdateExportSuccess,

    /// <summary>
    /// The channel is a channel that had already been exported and hasn't been appended as there are no new messages.
    /// </summary>
    UpdateExportSkip,

    /// <summary>
    /// The channel couldn't be exported successfully.
    /// </summary>
    ExportError,
}
