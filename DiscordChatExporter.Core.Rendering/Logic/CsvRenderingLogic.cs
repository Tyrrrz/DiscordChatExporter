using System.Linq;
using System.Text;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

using static DiscordChatExporter.Core.Rendering.Logic.SharedRenderingLogic;

namespace DiscordChatExporter.Core.Rendering.Logic
{
    public static class CsvRenderingLogic
    {
        // Header is always the same
        public static string FormatHeader(RenderContext context) => "AuthorID,Author,Date,Content,Attachments,Reactions";

        private static string EncodeValue(string value)
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        public static string FormatMarkdown(RenderContext context, string markdown) =>
            PlainTextRenderingLogic.FormatMarkdown(context, markdown);

        public static string FormatMessage(RenderContext context, Message message)
        {
            var buffer = new StringBuilder();

            buffer
                .Append(EncodeValue(message.Author.Id)).Append(',')
                .Append(EncodeValue(message.Author.FullName)).Append(',')
                .Append(EncodeValue(FormatDate(message.Timestamp, context.DateFormat))).Append(',')
                .Append(EncodeValue(FormatMarkdown(context, message.Content ?? ""))).Append(',')
                .Append(EncodeValue(message.Attachments.Select(a => a.Url).JoinToString(","))).Append(',')
                .Append(EncodeValue(message.Reactions.Select(r => $"{r.Emoji.Name} ({r.Count})").JoinToString(",")));

            return buffer.ToString();
        }
    }
}