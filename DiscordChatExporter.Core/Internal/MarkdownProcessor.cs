using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Internal
{
    internal static class MarkdownProcessor
    {
        public static string ToHtml(string markdown, bool allowLinks = false)
        {
            // HTML-encode content
            var output = markdown.HtmlEncode();

            // Encode multiline codeblocks (```text```)
            output = Regex.Replace(output,
                @"```+(?:[^`]*?\n)?([^`]+)\n?```+",
                m => $"\x1AM{m.Groups[1].Value.Base64Encode()}\x1AM");

            // Encode inline codeblocks (`text`)
            output = Regex.Replace(output,
                @"`([^`]+)`",
                m => $"\x1AI{m.Groups[1].Value.Base64Encode()}\x1AI");

            // Encode links
            if (allowLinks)
            {
                output = Regex.Replace(output, @"\[(.*?)\]\((.*?)\)",
                    m => $"\x1AL{m.Groups[1].Value.Base64Encode()}|{m.Groups[2].Value.Base64Encode()}\x1AL");
            }

            // Encode URLs
            output = Regex.Replace(output,
                @"(\b(?:(?:https?|ftp|file)://|www\.|ftp\.)(?:\([-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];])*(?:\([-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%=~_|$]))",
                m => $"\x1AU{m.Groups[1].Value.Base64Encode()}\x1AU");

            // Process bold (**text**)
            output = Regex.Replace(output, @"(\*\*)(?=\S)(.+?[*_]*)(?<=\S)\1", "<b>$2</b>");

            // Process underline (__text__)
            output = Regex.Replace(output, @"(__)(?=\S)(.+?)(?<=\S)\1", "<u>$2</u>");

            // Process italic (*text* or _text_)
            output = Regex.Replace(output, @"(\*|_)(?=\S)(.+?)(?<=\S)\1", "<i>$2</i>");

            // Process strike through (~~text~~)
            output = Regex.Replace(output, @"(~~)(?=\S)(.+?)(?<=\S)\1", "<s>$2</s>");

            // Decode and process multiline codeblocks
            output = Regex.Replace(output, "\x1AM(.*?)\x1AM",
                m => $"<div class=\"pre\">{m.Groups[1].Value.Base64Decode()}</div>");

            // Decode and process inline codeblocks
            output = Regex.Replace(output, "\x1AI(.*?)\x1AI",
                m => $"<span class=\"pre\">{m.Groups[1].Value.Base64Decode()}</span>");

            // Decode and process links
            if (allowLinks)
            {
                output = Regex.Replace(output, "\x1AL(.*?)|(.*?)\x1AL",
                    m => $"<a href=\"{m.Groups[2].Value.Base64Decode()}\">{m.Groups[1].Value.Base64Decode()}</a>");
            }

            // Decode and process URLs
            output = Regex.Replace(output, "\x1AU(.*?)\x1AU",
                m => $"<a href=\"{m.Groups[1].Value.Base64Decode()}\">{m.Groups[1].Value.Base64Decode()}</a>");

            // Process new lines
            output = output.Replace("\n", "<br />");

            return output;
        }
    }
}