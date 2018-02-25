using DiscordChatExporter.Core.Services;
using GalaSoft.MvvmLight;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.ViewModels
{
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettingsService _settingsService;

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

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
    }
}