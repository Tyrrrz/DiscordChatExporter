using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.ViewModels.Components;
using PowerKit.Extensions;

namespace DiscordChatExporter.Gui.Views.Components;

public partial class DashboardView : UserControl<DashboardViewModel>
{
    public DashboardView() => InitializeComponent();

    private void UserControl_OnLoaded(object? sender, RoutedEventArgs args) =>
        TokenValueTextBox.Focus();

    private void AvailableGuildsListBox_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs args
    ) => DataContext.PullChannelsCommand.ExecuteIfCan(null);

    private void AvailableChannelsTreeView_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs args
    )
    {
        // Hack: unselect categories because they cannot be exported
        foreach (
            var item in args.AddedItems.OfType<ChannelConnection>().Where(x => x.Channel.IsCategory)
        )
        {
            if (AvailableChannelsTreeView.TreeContainerFromItem(item) is TreeViewItem container)
                container.IsSelected = false;
        }
    }

    private void ChannelGrid_OnDoubleTapped(object? sender, TappedEventArgs args)
    {
        if (DataContext.SelectedChannels.Count != 1)
            return;

        DataContext.ExportCommand.ExecuteIfCan(null);
    }
}
