using System.Text.RegularExpressions;

namespace DiscordChatExporter.Domain.Markdown.Matching
{
    internal readonly struct StringPart
    {
        public string Target { get; }

        public int StartIndex { get; }

        public int Length { get; }

        public int EndIndex { get; }

        public StringPart(string target, int startIndex, int length)
        {
            Target = target;
            StartIndex = startIndex;
            Length = length;
            EndIndex = startIndex + length;
        }

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