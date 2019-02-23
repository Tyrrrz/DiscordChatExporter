using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sprache;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    // The following parsing logic is intentionally built in such way to make it resemble Discord's own
    // parsing as closely as possible, including its numerous flaws and inconsistencies
    internal static class Grammar
    {
        /* Formatting */

        // May contain italics inside, so capture until the last double asterisk that isn't followed by an asterisk
        private static readonly Parser<Node> BoldFormattedNode =
            Parse.RegexMatch(new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Bold, BuildTree(s)));

        // TODO: *asd**
        // TODO: ***s**a**b***
        // TODO: one *two **three** __four__* _five_ ***six***
        // Cannot have whitespace right after opening or right before closing token
        private static readonly Parser<Node> ItalicFormattedNode =
            Parse.RegexMatch(new Regex("\\*(?!\\s)(.+?)(?<!\\s)\\*", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        // Opening/closing token cannot come after/before word characters
        private static readonly Parser<Node> ItalicAltFormattedNode =
            Parse.RegexMatch(new Regex("(?<!\\w)_(.+?)_(?!\\w)", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        // May contain italics inside, so capture until the last double underline that isn't followed by an underline
        private static readonly Parser<Node> UnderlineFormattedNode =
            Parse.RegexMatch(new Regex("__(.+?)__(?!_)", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Underline, BuildTree(s)));

        // Strikethrough is safe
        private static readonly Parser<Node> StrikethroughFormattedNode =
            Parse.RegexMatch(new Regex("~~(.+?)~~", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Strikethrough, BuildTree(s)));

        // Spoiler is safe
        private static readonly Parser<Node> SpoilerFormattedNode =
            Parse.RegexMatch(new Regex("\\|\\|(.+?)\\|\\|", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Spoiler, BuildTree(s)));

        private static readonly Parser<Node> AnyFormattedNode = BoldFormattedNode
            .Or(UnderlineFormattedNode).Or(ItalicFormattedNode).Or(ItalicAltFormattedNode) // italic should be after bold & underline
            .Or(StrikethroughFormattedNode).Or(SpoilerFormattedNode);

        /* Code blocks */

        // Can't have backticks inside and surrounding whitespace is trimmed
        private static readonly Parser<Node> InlineCodeBlockNode =
            Parse.RegexMatch(new Regex("`\\s*([^`]+?)\\s*`", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new InlineCodeBlockNode(s));

        // The first word is a language identifier if it's the only word followed by a newline, the rest is code
        private static readonly Parser<Node> MultilineCodeBlockNode =
            Parse.RegexMatch(new Regex("```(?:(\\w*?)?(?:\\s*?\\n))?(.+)```", RegexOptions.Singleline))
                .Select(m => new MultilineCodeBlockNode(m.Groups[1].Value, m.Groups[2].Value));

        private static readonly Parser<Node> AnyCodeBlockNode = MultilineCodeBlockNode.Or(InlineCodeBlockNode); // inline should be after multiline

        /* Mentions */

        // @everyone
        private static readonly Parser<Node> EveryoneMentionNode = Parse.RegexMatch("@everyone")
            .Select(s => new MentionNode("everyone", MentionType.Meta));

        // @here
        private static readonly Parser<Node> HereMentionNode = Parse.RegexMatch("@here")
            .Select(s => new MentionNode("here", MentionType.Meta));

        // <@123456> or <@!123456>
        private static readonly Parser<Node> UserMentionNode = Parse.RegexMatch("<@!?(\\d+)>")
            .Select(m => m.Groups[1].Value).Select(s => new MentionNode(s, MentionType.User));

        // <#123456>
        private static readonly Parser<Node> ChannelMentionNode = Parse.RegexMatch("<#(\\d+)>")
            .Select(m => m.Groups[1].Value).Select(s => new MentionNode(s, MentionType.Channel));

        // <@&123456>
        private static readonly Parser<Node> RoleMentionNode = Parse.RegexMatch("<@&(\\d+)>")
            .Select(m => m.Groups[1].Value).Select(s => new MentionNode(s, MentionType.Role));

        private static readonly Parser<Node> AnyMentionNode = EveryoneMentionNode.Or(HereMentionNode)
            .Or(UserMentionNode).Or(ChannelMentionNode).Or(RoleMentionNode);

        /* Emojis */

        // <:lul:123456> or <a:lul:123456>
        private static readonly Parser<Node> EmojiNode = Parse.RegexMatch("<(a)?:(.+):(\\d+)>")
            .Select(m => new EmojiNode(m.Groups[3].Value, m.Groups[2].Value, m.Groups[1].Value.IsNotBlank()));

        private static readonly Parser<Node> AnyEmojiNode = EmojiNode;

        /* Links */

        // [title](link)
        private static readonly Parser<Node> TitledLinkNode = Parse.RegexMatch("\\[(.+)\\]\\((.+)\\)")
            .Select(m => new LinkNode(m.Groups[2].Value, m.Groups[1].Value));

        // http://blablabla/blabla without spaces in the middle and stop at the first punctuation mark
        private static readonly Parser<Node> AutoLinkNode = Parse.RegexMatch("(https?://\\S*[^\\.,;\"\'\\s])")
            .Select(m => m.Groups[1].Value)
            .Select(s => new LinkNode(s));

        // <http://blablabla/blabla>
        private static readonly Parser<Node> HiddenLinkNode =
            from open in Parse.Char('<')
            from link in AutoLinkNode
            from close in Parse.Char('>')
            select link;

        private static readonly Parser<Node> AnyLinkNode = TitledLinkNode.Or(HiddenLinkNode).Or(AutoLinkNode); // auto should be after hidden

        /* Text */

        // Shrug is an exception and needs to be exempt from formatting
        private static readonly Parser<Node> ShrugTextNode =
            Parse.String("¯\\_(ツ)_/¯").Text().Select(s => new TextNode(s));

        // Backslash escapes any following non-whitespace character except for digits and latin letters
        private static readonly Parser<Node> EscapedTextNode =
            Parse.RegexMatch("\\\\([^a-zA-Z0-9\\s])")
                .Select(m => m.Groups[1].Value)
                .Select(s => new TextNode(s));

        private static readonly Parser<Node> AnyTextNode = ShrugTextNode.Or(EscapedTextNode); // escaped should be after shrug

        /* Aggregator and fallback */

        // Any node recognized by above patterns
        private static readonly Parser<Node> AnyRecognizedNode = AnyFormattedNode.Or(AnyCodeBlockNode)
            .Or(AnyMentionNode).Or(AnyEmojiNode).Or(AnyLinkNode).Or(AnyTextNode);

        // Any node not recognized by above patterns (treated as plain text)
        private static readonly Parser<Node> FallbackNode =
            Parse.AnyChar.Except(AnyRecognizedNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        // Any node
        private static readonly Parser<Node> AnyNode = AnyRecognizedNode.Or(FallbackNode);

        // Entry point
        public static IReadOnlyList<Node> BuildTree(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}