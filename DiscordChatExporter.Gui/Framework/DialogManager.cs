﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Avalonia;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using DiscordChatExporter.Gui.Utils.Extensions;

namespace DiscordChatExporter.Gui.Framework;

public class DialogManager : IDisposable
{
    private readonly AsyncNonKeyedLocker _dialogLock = new();

    public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialog)
    {
        using (await _dialogLock.LockAsync())
        {
            await DialogHost.Show(
                dialog,
                // It's fine to await in a void method here because it's an event handler
                // ReSharper disable once AsyncVoidLambda
                async (object _, DialogOpenedEventArgs args) =>
                {
                    await dialog.WaitForCloseAsync();

                    try
                    {
                        args.Session.Close();
                    }
                    catch (InvalidOperationException)
                    {
                        // Dialog host is already processing a close operation
                    }
                }
            );

            return dialog.DialogResult;
        }
    }

    public async Task<string?> PromptSaveFilePathAsync(
        IReadOnlyList<FilePickerFileType>? fileTypes = null,
        string defaultFilePath = ""
    )
    {
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                FileTypeChoices = fileTypes,
                SuggestedFileName = defaultFilePath,
                DefaultExtension = Path.GetExtension(defaultFilePath).TrimStart('.')
            }
        );

        return file?.Path.LocalPath;
    }

    public async Task<string?> PromptDirectoryPathAsync(string defaultDirPath = "")
    {
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        var startLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(
            defaultDirPath
        );

        var folderPickResult = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = startLocation
            }
        );

        return folderPickResult.FirstOrDefault()?.Path.LocalPath;
    }

    public void Dispose() => _dialogLock.Dispose();
}
