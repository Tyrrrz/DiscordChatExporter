using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.ViewModels.Framework;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs
{
    public class SettingsViewModel : DialogScreen
    {
        private readonly SettingsService _settingsService;

        public bool IsAutoUpdateEnabled
        {
            get => _settingsService.IsAutoUpdateEnabled;
            set => _settingsService.IsAutoUpdateEnabled = value;
        }

        public string DateFormat
        {
            get => _settingsService.DateFormat;
            set => _settingsService.DateFormat = value;
        }

        public int MessageGroupLimit
        {
            get => _settingsService.MessageGroupLimit;
            set => _settingsService.MessageGroupLimit = value.ClampMin(0);
        }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }
    }
}