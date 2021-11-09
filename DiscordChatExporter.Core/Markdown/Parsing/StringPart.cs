using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Parsing
{
    internal readonly record struct StringPart(string Target, int StartIndex, int Length)
    {
        public int EndIndex => StartIndex + Length;

        public StringPart(string target)
            : this(target, 0, target.Length)
        {
        }

        public StringPart Slice(int newStartIndex, int newLength) => new(Target, newStartIndex, newLength);

        public StringPart Slice(int newStartIndex) => Slice(newStartIndex, EndIndex - newStartIndex);

        public StringPart Slice(Capture capture) => Slice(capture.Index, capture.Length);

        public override string ToString() => Target.Substring(StartIndex, Length);
    }
}