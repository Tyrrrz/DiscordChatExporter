using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DiscordChatExporter.Gui.Framework;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    ~ViewModelBase() => Dispose(false);

    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
