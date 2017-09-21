﻿using System.Collections.Generic;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.ViewModels
{
    public interface ISettingsViewModel
    {
        IReadOnlyList<Theme> AvailableThemes { get; }
        Theme Theme { get; set; }
    }
}