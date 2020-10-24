﻿using System;
using System.IO;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Stylet;

namespace DiscordChatExporter.Gui.ViewModels.Framework
{
    public class DialogManager
    {
        private readonly IViewManager _viewManager;

        public DialogManager(IViewManager viewManager)
        {
            _viewManager = viewManager;
        }

        public async ValueTask<T> ShowDialogAsync<T>(DialogScreen<T> dialogScreen)
        {
            var view = _viewManager.CreateAndBindViewForModelIfNecessary(dialogScreen);

            void OnDialogOpened(object? sender, DialogOpenedEventArgs openArgs)
            {
                void OnScreenClosed(object? o, EventArgs closeArgs)
                {
                    openArgs.Session.Close();
                    dialogScreen.Closed -= OnScreenClosed;
                }

                dialogScreen.Closed += OnScreenClosed;
            }

            await DialogHost.Show(view, OnDialogOpened);

            return dialogScreen.DialogResult;
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
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = defaultDirPath
            };

            return dialog.ShowDialog() == true ? dialog.SelectedPath : null;
        }
    }
}