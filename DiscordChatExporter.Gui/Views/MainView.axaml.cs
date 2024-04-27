using Avalonia.Interactivity;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.ViewModels;

namespace DiscordChatExporter.Gui.Views;

public partial class MainView : Window<MainViewModel>
{
    public MainView() => InitializeComponent();

    private void DialogHost_OnLoaded(object? sender, RoutedEventArgs args) =>
        DataContext.InitializeCommand.Execute(null);
}
