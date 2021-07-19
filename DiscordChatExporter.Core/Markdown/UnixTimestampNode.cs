using System;
using System.Diagnostics.CodeAnalysis;

namespace DiscordChatExporter.Core.Markdown
{
    internal class UnixTimestampNode : MarkdownNode
    {
        public DateTimeOffset Value { get; }

        public UnixTimestampNode(DateTimeOffset value) => Value = value;

        [ExcludeFromCodeCoverage]
        public override string ToString() => Value.ToString();
    }
}