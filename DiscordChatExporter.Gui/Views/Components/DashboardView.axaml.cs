using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DiscordChatExporter.Core.Discord.Data;
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

    private void AvailableGuildsListBox_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs args
    ) => DataContext.PullChannelsCommand.Execute(null);

    private void AvailableGuildsListBox_OnDoubleTapped(object? sender, TappedEventArgs args)
    {
        // Only works when there is exactly one non-category, child-less channel
        if (
            DataContext.AvailableChannels is not { Count: 1 }
            || DataContext.AvailableChannels[0].Channel.IsCategory
            || DataContext.AvailableChannels[0].Children.Count != 0
        )
            return;

        DataContext.SelectedChannels.Clear();
        DataContext.SelectedChannels.Add(DataContext.AvailableChannels[0]);
        DataContext.ExportCommand.Execute(null);
    }

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
}
