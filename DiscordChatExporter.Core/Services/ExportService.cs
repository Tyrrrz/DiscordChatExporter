using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

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
                    await ExportAsHtmlDarkAsync(log, output);
                }
                else if (format == ExportFormat.HtmlLight)
                {
                    await ExportAsHtmlLightAsync(log, output);
                }
                else if (format == ExportFormat.Csv)
                {
                    await ExportAsCsvAsync(log, output);
                }

                else throw new ArgumentOutOfRangeException(nameof(format));
            }
        }
    }

    public partial class ExportService
    {
        // TODO: use bytesize
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