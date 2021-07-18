using System;

namespace DiscordChatExporter.Core.Markdown
{
    internal class UnixTimestampNode : MarkdownNode
    {
        public DateTimeOffset Value { get; }

        public UnixTimestampNode(DateTimeOffset value) => Value = value;

        public override string ToString() => Value.ToString();
    }
}