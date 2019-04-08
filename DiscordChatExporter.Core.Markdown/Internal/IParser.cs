using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal interface IParser<T>
    {
        IReadOnlyList<ParseResult<T>> Parse(string input, int startIndex, int length);
    }
}