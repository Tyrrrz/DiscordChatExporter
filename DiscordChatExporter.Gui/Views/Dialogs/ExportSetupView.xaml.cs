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
            // Show advanced options by default if any of the advanced options were configured
            if (DataContext is ExportSetupViewModel vm)
            {
                AdvancedToggleButton.IsChecked =
                    vm.After != default ||
                    vm.Before != default ||
                    vm.PartitionLimit != default ||
                    vm.IsMediaExported != default;
            }
        }
    }
}