﻿using System;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Cli.ViewModels
{
    public interface IMainViewModel
    {
        Task ExportAsync(string token, string channelId, string filePath, ExportFormat format, DateTime? from,
            DateTime? to);
    }
}