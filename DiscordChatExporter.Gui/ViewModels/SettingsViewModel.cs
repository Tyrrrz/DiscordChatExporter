using DiscordChatExporter.Core.Services;
using GalaSoft.MvvmLight;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.ViewModels
{
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettingsService _settingsService;

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