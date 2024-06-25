using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordChatExporter.Gui.Framework;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs;

public partial class QuickExportViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string? _query;

    public bool CanProcessQuery => !String.IsNullOrWhiteSpace(Query);

    [RelayCommand(CanExecute = nameof(CanProcessQuery))]
    public void ProcessQuery() { }
}
