using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService : IExportService
    {
        private readonly ISettingsService _settingsService;

        public ExportService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task ExportAsync(ExportFormat format, string filePath, ChannelChatLog log)
        {
            using (var output = File.CreateText(filePath))
            {
                if (format == ExportFormat.PlainText)
                {
                    await ExportAsPlainTextAsync(log, output);
                }

                else if (format == ExportFormat.HtmlDark)
                {
                    var css = Assembly.GetExecutingAssembly()
                        .GetManifestResourceString("DiscordChatExporter.Core.Resources.ExportService.DarkTheme.css");
                    await ExportAsHtmlAsync(log, output, css);
                }

                else if (format == ExportFormat.HtmlLight)
                {
                    var css = Assembly.GetExecutingAssembly()
                        .GetManifestResourceString("DiscordChatExporter.Core.Resources.ExportService.LightTheme.css");
                    await ExportAsHtmlAsync(log, output, css);
                }

                else throw new ArgumentOutOfRangeException(nameof(format));
            }
        }
    }

    public partial class ExportService
    {
        private static string Base64Encode(string str)
        {
            return str.GetBytes().ToBase64();
        }

        private static string Base64Decode(string str)
        {
            return str.FromBase64().GetString();
        }

        private static string HtmlEncode(string str)
        {
            return WebUtility.HtmlEncode(str);
        }

        private static string FormatFileSize(long fileSize)
        {
            string[] units = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
            double size = fileSize;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }
    }
}