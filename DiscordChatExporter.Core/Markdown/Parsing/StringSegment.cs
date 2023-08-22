using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal readonly record struct StringSegment(string Source, int StartIndex, int Length)
{
    public int EndIndex => StartIndex + Length;

    public StringSegment(string target)
        : this(target, 0, target.Length) { }

    public StringSegment Relocate(int newStartIndex, int newLength) =>
        new(Source, newStartIndex, newLength);

    public StringSegment Relocate(Capture capture) => Relocate(capture.Index, capture.Length);

    public override string ToString() => Source.Substring(StartIndex, Length);
}
