using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Markdown.Internal;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown
{
    // The following parsing logic is meant to replicate Discord's markdown grammar as close as possible
    public static class MarkdownParser
    {
        private const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        /* Formatting */

        // Capture until the earliest double asterisk not followed by an asterisk
        private static readonly IMatcher<Node> BoldFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "**", TextFormatting.Bold, Parse(m.Groups[1].Value)));

        // Capture until the earliest single asterisk not preceded or followed by an asterisk
        // Can't have whitespace right after opening or right before closing asterisk
        private static readonly IMatcher<Node> ItalicFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\*(?!\\s)(.+?)(?<!\\s|\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "*", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Can't have underscores inside
        // Can't have word characters right after closing underscore
        private static readonly IMatcher<Node> ItalicAltFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("_([^_]+?)_(?!\\w)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "_", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple asterisk not preceded or followed by an asterisk
        private static readonly IMatcher<Node> ItalicBoldFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\*(\\*\\*(?:.+?)\\*\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "*", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Capture until the earliest double underscore not followed by an underscore
        private static readonly IMatcher<Node> UnderlineFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("__(.+?)__(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "__", TextFormatting.Underline, Parse(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple underscore not preceded or followed by an underscore
        private static readonly IMatcher<Node> ItalicUnderlineFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("_(__(?:.+?)__)_(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "_", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Strikethrough is safe
        private static readonly IMatcher<Node> StrikethroughFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("~~(.+?)~~", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "~~", TextFormatting.Strikethrough, Parse(m.Groups[1].Value)));

        // Spoiler is safe
        private static readonly IMatcher<Node> SpoilerFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\|\\|(.+?)\\|\\|", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "||", TextFormatting.Spoiler, Parse(m.Groups[1].Value)));

        /* Code blocks */

        // Can't have backticks inside and surrounding whitespace is trimmed
        private static readonly IMatcher<Node> InlineCodeBlockNodeMatcher =
            new RegexMatcher<Node>(new Regex("`\\s*([^`]+?)\\s*`", DefaultRegexOptions | RegexOptions.Singleline),
                m => new InlineCodeBlockNode(m.Value, m.Groups[1].Value));

        // The first word is a language identifier if it's the only word followed by a newline, the rest is code
        private static readonly IMatcher<Node> MultilineCodeBlockNodeMatcher =
            new RegexMatcher<Node>(new Regex("```(?:(\\w*?)?(?:\\s*?\\n))?(.+?)```", DefaultRegexOptions | RegexOptions.Singleline),
                m => new MultilineCodeBlockNode(m.Value, m.Groups[1].Value, m.Groups[2].Value));

        /* Mentions */

        // @everyone or @here
        private static readonly IMatcher<Node> MetaMentionNodeMatcher =
            new RegexMatcher<Node>(new Regex("@(everyone|here)", DefaultRegexOptions),
                m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Meta));

        // <@123456> or <@!123456>
        private static readonly IMatcher<Node> UserMentionNodeMatcher =
            new RegexMatcher<Node>(new Regex("<@!?(\\d+)>", DefaultRegexOptions),
                m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.User));

        // <#123456>
        private static readonly IMatcher<Node> ChannelMentionNodeMatcher =
            new RegexMatcher<Node>(new Regex("<#(\\d+)>", DefaultRegexOptions),
                m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Channel));

        // <@&123456>
        private static readonly IMatcher<Node> RoleMentionNodeMatcher =
            new RegexMatcher<Node>(new Regex("<@&(\\d+)>", DefaultRegexOptions),
                m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Role));

        /* Emojis */

        // Matches all standard unicode emojis
        private static readonly IMatcher<Node> StandardEmojiNodeMatcher =
            new RegexMatcher<Node>(new Regex(
                "([\\u2700-\\u27bf]|" +
                "(?:\\ud83c[\\udde6-\\uddff]){2}|" +
                "[\\ud800-\\udbff][\\udc00-\\udfff]|" +
                "[\\u0023-\\u0039]\\u20e3|" +
                "\\u3299|\\u3297|\\u303d|\\u3030|\\u24c2|\\ud83c[\\udd70-\\udd71]|\\ud83c[\\udd7e-\\udd7f]|\\ud83c\\udd8e|\\ud83c[\\udd91-\\udd9a]|\\ud83c[\\udde6-\\uddff]|" +
                "\\ud83c[\\ude01-\\ude02]|\\ud83c\\ude1a|\\ud83c\\ude2f|\\ud83c[\\ude32-\\ude3a]|\\ud83c[\\ude50-\\ude51]|\\u203c|\\u2049|[\\u25aa-\\u25ab]|" +
                "\\u25b6|\\u25c0|[\\u25fb-\\u25fe]|\\u00a9|\\u00ae|\\u2122|\\u2139|\\ud83c\\udc04|[\\u2600-\\u26FF]|\\u2b05|\\u2b06|\\u2b07|\\u2b1b|\\u2b1c|\\u2b50|" +
                "\\u2b55|\\u231a|\\u231b|\\u2328|\\u23cf|[\\u23e9-\\u23f3]|[\\u23f8-\\u23fa]|\\ud83c\\udccf|\\u2934|\\u2935|[\\u2190-\\u21ff])", DefaultRegexOptions),
                m => new EmojiNode(m.Value, m.Groups[1].Value));

        // <:lul:123456> or <a:lul:123456>
        private static readonly IMatcher<Node> CustomEmojiNodeMatcher =
            new RegexMatcher<Node>(new Regex("<(a)?:(.+?):(\\d+)>", DefaultRegexOptions),
                m => new EmojiNode(m.Value, m.Groups[3].Value, m.Groups[2].Value, !m.Groups[1].Value.IsEmpty()));

        /* Links */

        // [title](link)
        private static readonly IMatcher<Node> TitledLinkNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\[(.+?)\\]\\((.+?)\\)", DefaultRegexOptions),
                m => new LinkNode(m.Value, m.Groups[2].Value, m.Groups[1].Value));

        // Starts with http:// or https://, stops at the last non-whitespace character followed by whitespace or punctuation character
        private static readonly IMatcher<Node> AutoLinkNodeMatcher =
            new RegexMatcher<Node>(new Regex("(https?://\\S*[^\\.,:;\"\'\\s])", DefaultRegexOptions),
                m => new LinkNode(m.Value, m.Groups[1].Value));

        // Autolink surrounded by angular brackets
        private static readonly IMatcher<Node> HiddenLinkNodeMatcher =
            new RegexMatcher<Node>(new Regex("<(https?://\\S*[^\\.,:;\"\'\\s])>", DefaultRegexOptions),
                m => new LinkNode(m.Value, m.Groups[1].Value));

        /* Text */

        // Shrug is an exception and needs to be exempt from formatting
        private static readonly IMatcher<Node> ShrugTextNodeMatcher = new StringMatcher<Node>(@"¯\_(ツ)_/¯", s => new TextNode(s));

        // Backslash escapes any following unicode surrogate pair
        //private static readonly Parser<Node> EscapedSurrogateTextNode =
        //    from slash in Parse.Char('\\')
        //    from high in Parse.AnyChar.Where(char.IsHighSurrogate)
        //    from low in Parse.AnyChar
        //    let lexeme = $"{slash}{high}{low}"
        //    let text = $"{high}{low}"
        //    select new TextNode(lexeme, text);

        // Backslash escapes any following non-whitespace character except for digits and latin letters
        private static readonly IMatcher<Node> EscapedTextNodeMatcher = 
            new RegexMatcher<Node>(new Regex("\\\\([^a-zA-Z0-9\\s])", DefaultRegexOptions),
                m => new TextNode(m.Value, m.Groups[1].Value));

        private static readonly IMatcher<Node> NodeMatcher = new AggregateMatcher<Node>(
            ItalicBoldFormattedNodeMatcher,
            ItalicUnderlineFormattedNodeMatcher,
            BoldFormattedNodeMatcher,
            ItalicFormattedNodeMatcher,
            UnderlineFormattedNodeMatcher,
            ItalicAltFormattedNodeMatcher,
            StrikethroughFormattedNodeMatcher,
            SpoilerFormattedNodeMatcher,
            MultilineCodeBlockNodeMatcher,
            InlineCodeBlockNodeMatcher,
            MetaMentionNodeMatcher,
            UserMentionNodeMatcher,
            ChannelMentionNodeMatcher,
            RoleMentionNodeMatcher,
            StandardEmojiNodeMatcher,
            CustomEmojiNodeMatcher,
            TitledLinkNodeMatcher,
            AutoLinkNodeMatcher,
            HiddenLinkNodeMatcher,
            ShrugTextNodeMatcher,
            EscapedTextNodeMatcher);

        public static IReadOnlyList<Node> Parse(string input) => NodeMatcher.MatchAll(input, s => new TextNode(s)).Select(r => r.Value).ToArray();
    }
}