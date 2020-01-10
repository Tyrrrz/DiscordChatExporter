using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.ViewModels.Framework;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs
{
    public class SettingsViewModel : DialogScreen
    {
        private readonly SettingsService _settingsService;

        public string DateFormat
        {
            get => _settingsService.DateFormat;
            set => _settingsService.DateFormat = value;
        }

        public bool IsAutoUpdateEnabled
        {
            get => _settingsService.IsAutoUpdateEnabled;
            set => _settingsService.IsAutoUpdateEnabled = value;
        }

        public bool IsTokenPersisted
        {
            get => _settingsService.IsTokenPersisted;
            set => _settingsService.IsTokenPersisted = value;
        }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }
    }
}