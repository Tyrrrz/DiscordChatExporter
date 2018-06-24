﻿using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IExportService
    {
        void Export(ExportFormat format, string filePath, ChatLog log);
    }
}