using Avalonia.Interactivity;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.ViewModels.Dialogs;

namespace DiscordChatExporter.Gui.Views.Dialogs;

public partial class ExportSetupView : UserControl<ExportSetupViewModel>
{
    public ExportSetupView() => InitializeComponent();

    private void UserControl_OnLoaded(object? sender, RoutedEventArgs args) =>
        DataContext.InitializeCommand.Execute(null);
}
