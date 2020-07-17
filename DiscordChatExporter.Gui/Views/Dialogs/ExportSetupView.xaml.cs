using System.Windows;
using DiscordChatExporter.Gui.ViewModels.Dialogs;

namespace DiscordChatExporter.Gui.Views.Dialogs
{
    public partial class ExportSetupView
    {
        public ExportSetupView()
        {
            InitializeComponent();
        }

        private void AdvancedToggleButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExportSetupViewModel vm)
                AdvancedToggleButton.IsChecked = vm.IsAdvancedSectionDisplayedByDefault;
        }
    }
}