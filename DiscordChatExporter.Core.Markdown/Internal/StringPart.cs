namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal class StringPart
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

        public override string ToString() => Target.Substring(StartIndex, Length);
    }
}