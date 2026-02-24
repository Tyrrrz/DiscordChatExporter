using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DiscordChatExporter.Gui.Utils.Extensions;
using DiscordChatExporter.Gui.Views.Controls;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownInline = Markdig.Syntax.Inlines.Inline;

namespace DiscordChatExporter.Gui.Converters;

public class MarkdownToInlinesConverter : IValueConverter
{
    public static readonly MarkdownToInlinesConverter Instance = new();

    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras()
        .Build();

    private static void ProcessInline(
        InlineCollection inlines,
        MarkdownInline markdownInline,
        FontWeight? fontWeight = null,
        FontStyle? fontStyle = null,
        TextDecorationCollection? textDecorations = null
    )
    {
        switch (markdownInline)
        {
            case LiteralInline literal:
            {
                var run = new Run(literal.Content.ToString())
                {
                    BaselineAlignment = BaselineAlignment.Center,
                };

                if (fontWeight is not null)
                    run.FontWeight = fontWeight.Value;
                if (fontStyle is not null)
                    run.FontStyle = fontStyle.Value;
                if (textDecorations is not null)
                    run.TextDecorations = textDecorations;

                inlines.Add(run);
                break;
            }

            case LineBreakInline:
            {
                inlines.Add(new LineBreak());
                break;
            }

            case EmphasisInline emphasis:
            {
                var newWeight = fontWeight;
                var newStyle = fontStyle;
                var newDecorations = textDecorations;

                switch (emphasis.DelimiterChar)
                {
                    case '*' or '_' when emphasis.DelimiterCount == 2:
                        newWeight = FontWeight.SemiBold;
                        break;
                    case '*' or '_':
                        newStyle = FontStyle.Italic;
                        break;
                    case '~':
                        newDecorations = TextDecorations.Strikethrough;
                        break;
                    case '+':
                        newDecorations = TextDecorations.Underline;
                        break;
                }

                foreach (var child in emphasis)
                    ProcessInline(inlines, child, newWeight, newStyle, newDecorations);

                break;
            }

            case LinkInline link:
            {
                inlines.Add(new HyperLink { Text = link.GetInnerText(), Url = link.Url });
                break;
            }

            case ContainerInline container:
            {
                foreach (var child in container)
                    ProcessInline(inlines, child, fontWeight, fontStyle, textDecorations);

                break;
            }
        }
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var inlines = new InlineCollection();
        if (value is not string { Length: > 0 } text)
            return inlines;

        var isFirst = true;

        foreach (var block in Markdown.Parse(text, MarkdownPipeline))
        {
            switch (block)
            {
                case ParagraphBlock { Inline: not null } paragraph:
                {
                    if (!isFirst)
                    {
                        // Insert a blank line between paragraphs
                        inlines.Add(new LineBreak());
                        inlines.Add(new LineBreak());
                    }

                    isFirst = false;

                    foreach (var markdownInline in paragraph.Inline!)
                        ProcessInline(inlines, markdownInline);

                    break;
                }

                case ListBlock list:
                {
                    var itemOrder = 1;
                    if (list.IsOrdered && int.TryParse(list.OrderedStart, out var startNum))
                        itemOrder = startNum;

                    foreach (var listItem in list.OfType<ListItemBlock>())
                    {
                        if (!isFirst)
                            inlines.Add(new LineBreak());
                        isFirst = false;

                        var prefix = list.IsOrdered ? $"{itemOrder++}. " : $"{list.BulletType}  ";

                        inlines.Add(
                            new Run(prefix) { BaselineAlignment = BaselineAlignment.Center }
                        );

                        foreach (var subBlock in listItem.OfType<ParagraphBlock>())
                        {
                            if (subBlock is { Inline: not null })
                            {
                                foreach (var markdownInline in subBlock.Inline)
                                    ProcessInline(inlines, markdownInline);
                            }
                        }
                    }

                    break;
                }
            }
        }

        return inlines;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
