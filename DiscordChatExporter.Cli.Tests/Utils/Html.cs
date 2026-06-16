using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace DiscordChatExporter.Cli.Tests.Utils;

internal static class Html
{
    private static readonly IHtmlParser Parser = new HtmlParser();

    public static IHtmlDocument Parse(string source) => Parser.ParseDocument(source);
}
