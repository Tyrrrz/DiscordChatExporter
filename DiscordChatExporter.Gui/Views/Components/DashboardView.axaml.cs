using Avalonia.Input;
using Avalonia.Interactivity;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.ViewModels.Components;

namespace DiscordChatExporter.Gui.Views.Components;

public partial class DashboardView : UserControl<DashboardViewModel>
{
    public DashboardView() => InitializeComponent();

    private void UserControl_OnLoaded(object? sender, RoutedEventArgs args)
    {
        DataContext.InitializeCommand.Execute(null);
        TokenValueTextBox.Focus();
    }
    
    private void GuildPanel_OnPointerReleased(object? sender, PointerReleasedEventArgs args) =>
        DataContext.PullChannelsCommand.Execute(null);

    private void ChannelListBoxItemGrid_OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.ClickCount == 2)
        {
            if (DataContext.ExportCommand.CanExecute(null)) 
                DataContext.ExportCommand.Execute(null);
        }
    }
}
