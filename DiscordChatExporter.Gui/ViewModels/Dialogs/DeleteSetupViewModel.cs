using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Localization;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs;

public partial class DeleteSetupViewModel(LocalizationManager localizationManager)
    : DialogViewModelBase
{
    public LocalizationManager LocalizationManager { get; } = localizationManager;

    [ObservableProperty]
    public partial Guild? Guild { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSingleChannel))]
    public partial IReadOnlyList<Channel>? Channels { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAfterDateSet))]
    [NotifyPropertyChangedFor(nameof(After))]
    public partial DateTimeOffset? AfterDate { get; set; }

    [ObservableProperty]
    public partial TimeSpan? AfterTime { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBeforeDateSet))]
    [NotifyPropertyChangedFor(nameof(Before))]
    public partial DateTimeOffset? BeforeDate { get; set; }

    [ObservableProperty]
    public partial TimeSpan? BeforeTime { get; set; }

    public bool IsSingleChannel => Channels?.Count == 1;

    public bool IsAfterDateSet => AfterDate is not null;

    public DateTimeOffset? After => AfterDate?.Add(AfterTime ?? TimeSpan.Zero);

    public bool IsBeforeDateSet => BeforeDate is not null;

    public DateTimeOffset? Before => BeforeDate?.Add(BeforeTime ?? TimeSpan.Zero);

    [RelayCommand]
    private void Confirm()
    {
        Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        Close(null);
    }
}
