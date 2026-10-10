namespace DiscordChatExporter.Core.Exporting;

public enum ForumCommonAssetMode
{
    SharedFolder,
    PerThreadFolder,
    Skip,
}

public enum ForumAttachmentFolderMode
{
    PerThread,
    ByMediaType,
    ByMessage,
}

public enum ForumAttachmentNamingMode
{
    OriginalWithHash,
    AttachmentIdAndOriginal,
    MessageAndAttachmentIdsAndOriginal,
}
