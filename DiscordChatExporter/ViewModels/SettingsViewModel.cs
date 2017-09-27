using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Models;
using DiscordChatExporter.Services;
using GalaSoft.MvvmLight;

namespace DiscordChatExporter.ViewModels
{
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettingsService _settingsService;

        public IReadOnlyList<Theme> AvailableThemes { get; }

        public Theme Theme
        {
            get => _settingsService.Theme;
            set => _settingsService.Theme = value;
        }

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            // Defaults
            AvailableThemes = Enum.GetValues(typeof(Theme)).Cast<Theme>().ToArray();
        }
    }
}