using System;
using System.IO;
using System.Threading.Tasks;
using AsyncKeyedLock;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Stylet;

namespace DiscordChatExporter.Gui.ViewModels.Framework;

public class DialogManager(IViewManager viewManager) : IDisposable
{
    private readonly AsyncNonKeyedLocker _dialogLock = new();

    public async ValueTask<T?> ShowDialogAsync<T>(DialogScreen<T> dialogScreen)
    {
        var view = viewManager.CreateAndBindViewForModelIfNecessary(dialogScreen);

        void OnDialogOpened(object? openSender, DialogOpenedEventArgs openArgs)
        {
            void OnScreenClosed(object? closeSender, EventArgs closeArgs)
            {
                try
                {
                    openArgs.Session.Close();
                }
                catch (InvalidOperationException)
                {
                    // Race condition: dialog is already being closed
                }

                dialogScreen.Closed -= OnScreenClosed;
            }
            dialogScreen.Closed += OnScreenClosed;
        }

        using (await _dialogLock.LockAsync())
        {
            await DialogHost.Show(view, OnDialogOpened);
            return dialogScreen.DialogResult;
        }
    }

    public string? PromptSaveFilePath(string filter = "All files|*.*", string defaultFilePath = "")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            AddExtension = true,
            FileName = defaultFilePath,
            DefaultExt = Path.GetExtension(defaultFilePath)
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PromptDirectoryPath(string defaultDirPath = "")
    {
        var dialog = new OpenFolderDialog { InitialDirectory = defaultDirPath };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public void Dispose()
    {
        _dialogLock.Dispose();
    }
}
