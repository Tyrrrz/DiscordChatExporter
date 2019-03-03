using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sprache;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    // The following parsing logic is meant to replicate Discord's markdown grammar as close as possible
    internal static class Grammar
    {
        /* Formatting */

        // Capture until the earliest double asterisk not followed by an asterisk
        private static readonly Parser<Node> BoldFormattedNode =
            Parse.RegexMatch(new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "**", TextFormatting.Bold, BuildTree(m.Groups[1].Value)));

        // Capture until the earliest single asterisk not preceded or followed by an asterisk
        // Can't have whitespace right after opening or right before closing asterisk
        private static readonly Parser<Node> ItalicFormattedNode =
            Parse.RegexMatch(new Regex("\\*(?!\\s)(.+?)(?<!\\s|\\*)\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "*", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Can't have underscores inside
        // Can't have word characters right after closing underscore
        private static readonly Parser<Node> ItalicAltFormattedNode =
            Parse.RegexMatch(new Regex("_([^_]+?)_(?!\\w)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "_", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple asterisk not preceded or followed by an asterisk
        private static readonly Parser<Node> ItalicBoldFormattedNode =
            Parse.RegexMatch(new Regex("\\*(\\*\\*(?:.+?)\\*\\*)\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "*", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Capture until the earliest double underscore not followed by an underscore
        private static readonly Parser<Node> UnderlineFormattedNode =
            Parse.RegexMatch(new Regex("__(.+?)__(?!_)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "__", TextFormatting.Underline, BuildTree(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple underscore not preceded or followed by an underscore
        private static readonly Parser<Node> ItalicUnderlineFormattedNode =
            Parse.RegexMatch(new Regex("_(__(?:.+?)__)_(?!_)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "_", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Strikethrough is safe
        private static readonly Parser<Node> StrikethroughFormattedNode =
            Parse.RegexMatch(new Regex("~~(.+?)~~", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "~~", TextFormatting.Strikethrough, BuildTree(m.Groups[1].Value)));

        // Spoiler is safe
        private static readonly Parser<Node> SpoilerFormattedNode =
            Parse.RegexMatch(new Regex("\\|\\|(.+?)\\|\\|", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "||", TextFormatting.Spoiler, BuildTree(m.Groups[1].Value)));

        // Combinator, order matters
        private static readonly Parser<Node> AnyFormattedNode = 
            ItalicBoldFormattedNode.Or(ItalicUnderlineFormattedNode)
            .Or(BoldFormattedNode).Or(ItalicFormattedNode)
            .Or(UnderlineFormattedNode).Or(ItalicAltFormattedNode)
            .Or(StrikethroughFormattedNode).Or(SpoilerFormattedNode);

        /* Code blocks */

        // Can't have backticks inside and surrounding whitespace is trimmed
        private static readonly Parser<Node> InlineCodeBlockNode =
            Parse.RegexMatch(new Regex("`\\s*([^`]+?)\\s*`", RegexOptions.Singleline))
                .Select(m => new InlineCodeBlockNode(m.Value, m.Groups[1].Value));

        // The first word is a language identifier if it's the only word followed by a newline, the rest is code
        private static readonly Parser<Node> MultilineCodeBlockNode =
            Parse.RegexMatch(new Regex("```(?:(\\w*?)?(?:\\s*?\\n))?(.+)```", RegexOptions.Singleline))
                .Select(m => new MultilineCodeBlockNode(m.Value, m.Groups[1].Value, m.Groups[2].Value));

        // Combinator, order matters
        private static readonly Parser<Node> AnyCodeBlockNode = MultilineCodeBlockNode.Or(InlineCodeBlockNode);

        /* Mentions */

        // @everyone or @here
        private static readonly Parser<Node> MetaMentionNode = Parse.RegexMatch("@(everyone|here)")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Meta));

        // <@123456> or <@!123456>
        private static readonly Parser<Node> UserMentionNode = Parse.RegexMatch("<@!?(\\d+)>")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.User));

        // <#123456>
        private static readonly Parser<Node> ChannelMentionNode = Parse.RegexMatch("<#(\\d+)>")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Channel));

        // <@&123456>
        private static readonly Parser<Node> RoleMentionNode = Parse.RegexMatch("<@&(\\d+)>")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Role));

        // Combinator, order matters
        private static readonly Parser<Node> AnyMentionNode =
            MetaMentionNode.Or(UserMentionNode).Or(ChannelMentionNode).Or(RoleMentionNode);

        /* Emojis */

        // Matches all standard unicode emojis
        private static readonly Parser<Node> StandardEmojiNode = Parse.RegexMatch(
                "([\\u2700-\\u27bf]|" +
                "(?:\\ud83c[\\udde6-\\uddff]){2}|" +
                "[\\ud800-\\udbff][\\udc00-\\udfff]|" +
                "[\\u0023-\\u0039]\\u20e3|" +
                "\\u3299|\\u3297|\\u303d|\\u3030|\\u24c2|\\ud83c[\\udd70-\\udd71]|\\ud83c[\\udd7e-\\udd7f]|\\ud83c\\udd8e|\\ud83c[\\udd91-\\udd9a]|\\ud83c[\\udde6-\\uddff]|" +
                "[\\ud83c[\\ude01-\\ude02]|\\ud83c\\ude1a|\\ud83c\\ude2f|[\\ud83c[\\ude32-\\ude3a]|[\\ud83c[\\ude50-\\ude51]|\\u203c|\\u2049|[\\u25aa-\\u25ab]|" +
                "\\u25b6|\\u25c0|[\\u25fb-\\u25fe]|\\u00a9|\\u00ae|\\u2122|\\u2139|\\ud83c\\udc04|[\\u2600-\\u26FF]|\\u2b05|\\u2b06|\\u2b07|\\u2b1b|\\u2b1c|\\u2b50|" +
                "\\u2b55|\\u231a|\\u231b|\\u2328|\\u23cf|[\\u23e9-\\u23f3]|[\\u23f8-\\u23fa]|\\ud83c\\udccf|\\u2934|\\u2935|[\\u2190-\\u21ff])")
            .Select(m => new EmojiNode(m.Value, m.Groups[1].Value));

        // <:lul:123456> or <a:lul:123456>
        private static readonly Parser<Node> CustomEmojiNode = Parse.RegexMatch("<(a)?:(.+):(\\d+)>")
            .Select(m => new EmojiNode(m.Value, m.Groups[3].Value, m.Groups[2].Value, m.Groups[1].Value.IsNotBlank()));

        // Combinator, order matters
        private static readonly Parser<Node> AnyEmojiNode = StandardEmojiNode.Or(CustomEmojiNode);

        /* Links */

        // [title](link)
        private static readonly Parser<Node> TitledLinkNode = Parse.RegexMatch("\\[(.+)\\]\\((.+)\\)")
            .Select(m => new LinkNode(m.Value, m.Groups[2].Value, m.Groups[1].Value));

        // Starts with http:// or https://, stops at the last non-whitespace character followed by whitespace or punctuation character
        private static readonly Parser<Node> AutoLinkNode = Parse.RegexMatch("(https?://\\S*[^\\.,:;\"\'\\s])")
            .Select(m => new LinkNode(m.Value, m.Groups[1].Value));

        // Autolink surrounded by angular brackets
        private static readonly Parser<Node> HiddenLinkNode = Parse.RegexMatch("<(https?://\\S*[^\\.,:;\"\'\\s])>")
            .Select(m => new LinkNode(m.Value, m.Groups[1].Value));

        // Combinator, order matters
        private static readonly Parser<Node> AnyLinkNode = TitledLinkNode.Or(HiddenLinkNode).Or(AutoLinkNode); 

        /* Text */

        // Shrug is an exception and needs to be exempt from formatting
        private static readonly Parser<Node> ShrugTextNode =
            Parse.String("¯\\_(ツ)_/¯").Text().Select(s => new TextNode(s));

        // Backslash escapes any following unicode surrogate pair
        private static readonly Parser<Node> EscapedSurrogateTextNode =
            from slash in Parse.Char('\\')
            from high in Parse.AnyChar.Where(char.IsHighSurrogate)
            from low in Parse.AnyChar
            let lexeme = $"{slash}{high}{low}"
            let text = $"{high}{low}"
            select new TextNode(lexeme, text);

        // Backslash escapes any following non-whitespace character except for digits and latin letters
        private static readonly Parser<Node> EscapedTextNode =
            Parse.RegexMatch("\\\\([^a-zA-Z0-9\\s])").Select(m => new TextNode(m.Value, m.Groups[1].Value));

        // Combinator, order matters
        private static readonly Parser<Node> AnyTextNode = ShrugTextNode.Or(EscapedSurrogateTextNode).Or(EscapedTextNode);

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