using System.Linq;
using Avalonia;
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

    private void GuildListBoxItemPanel_OnPointerReleased(
        object? sender,
        PointerReleasedEventArgs args
    ) => DataContext.PullChannelsCommand.Execute(null);

    private void AvailableChannelsTreeView_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs args
    )
    {
        // Hack: unselect categories because they cannot be exported
        foreach (var item in args.AddedItems.OfType<ChannelNode>().Where(x => x.Channel.IsCategory))
        {
            if (AvailableChannelsTreeView.TreeContainerFromItem(item) is TreeViewItem container)
                container.IsSelected = false;
        }
    }

    private void ChannelTreeViewItemPanel_OnPointerPressed(
        object? sender,
        PointerPressedEventArgs args
    )
    {
        // Single click
        if (args.ClickCount == 1)
        {
            // Expand or collapse the tree view item
            if (
                sender is StyledElement { DataContext: { } item }
                && AvailableChannelsTreeView.TreeContainerFromItem(item) is TreeViewItem container
            )
            {
                container.IsExpanded = !container.IsExpanded;
            }
        }
        // Double click
        else if (args.ClickCount == 2)
        {
            // Shortcut to export the selected channels (except categories)
            if (sender is StyledElement { DataContext: ChannelNode { Channel.IsCategory: false } })
            {
                if (DataContext.ExportCommand.CanExecute(null))
                    DataContext.ExportCommand.Execute(null);
            }
        }
    }
}
