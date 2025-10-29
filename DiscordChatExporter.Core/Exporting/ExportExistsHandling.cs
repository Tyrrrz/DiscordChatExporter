namespace DiscordChatExporter.Core.Exporting;

/// <summary>
/// Represents the setting on how to handle the export of a channel that has already been exported.
/// </summary>
public enum ExportExistsHandling
{
    /// <summary>
    /// If a channel had previously been exported, its export will be aborted.
    /// </summary>
    Abort,

    /// <summary>
    /// If a channel had previously been exported, the existing export will be removed, and it will be exported again.
    /// </summary>
    Overwrite,

    /// <summary>
    /// If a channel had previously been exported, the existing export will be appended, which means that only messages
    /// after the last export will be exported.
    /// </summary>
    Append,
}
