namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal interface IMatcher<T>
    {
        ParsedMatch<T> Match(string input, int startIndex, int length);
    }
}