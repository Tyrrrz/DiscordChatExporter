namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal interface IMatcher<T>
    {
        ParsedMatch<T>? Match(StringPart stringPart);
    }
}