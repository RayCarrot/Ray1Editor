using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using NLog;

namespace RayCarrot.Ray1Editor;

public class AppUIManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Displays a message to the user
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="header">The header for the message</param>
    /// <param name="messageType">The type of message, determining its visual appearance</param>
    /// <param name="allowCancel">True if the option to cancel is present</param>
    /// <param name="additionalActions">Additional actions</param>
    /// <returns>True if the user accepted the message, otherwise false</returns>
    public bool DisplayMessage(string message, string header, DialogMessageType messageType, bool allowCancel = false, IList<DialogMessageActionViewModel> additionalActions = null)
    {
        // Make sure the application has been set up
        if (Application.Current.Dispatcher == null)
            throw new Exception("A message box can not be shown when the application dispatcher is null");

        Logger.Log(LogLevel.Info, $"Logging message with content: {message} of type {messageType}");

        // Create the message actions
        var actions = new ObservableCollection<DialogMessageActionViewModel>();

        // Create a cancel action if available
        if (allowCancel)
            actions.Add(new DialogMessageActionViewModel()
            {
                DisplayText = "Cancel",
                DisplayDescription = "Cancel",
                IsCancel = true,
                ActionResult = false
            });

        // Add additional actions
        if (additionalActions != null)
            actions.AddRange(additionalActions);

        // Create the default action
        actions.Add(new DialogMessageActionViewModel()
        {
            DisplayText = "Ok",
            DisplayDescription = "Ok",
            IsDefault = true,
            ActionResult = true
        });

        // Run on the UI thread
        return App.Current.Dispatcher.Invoke(() =>
        {
            // Create the view model
            var vm = new DialogMessageViewModel()
            {
                MessageText = message,
                Title = header ?? $"{messageType}",
                MessageType = messageType,
                DialogImageSource = new ImageSourceConverter().ConvertFromString($"{App.WPFAppBasePath}UI/Img/{messageType}.png") as ImageSource,
                DialogActions = actions,
                DefaultActionResult = false
            };

            // Create the window
            var win = new DialogMessageWindow()
            {
                DataContext = vm
            };

            // Show the window dialog
            win.ShowDialog();

            return win.DialogResult ?? false;
        });
    }

    public string GetSaveFilePath(string header, string defaultFileName, string defaultExtension, string filter)
    {
        var saveFileDialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = defaultExtension,
            FileName = defaultFileName,
            Title = header,
            ValidateNames = true,
            Filter = filter,
            OverwritePrompt = true
        };

        var result = saveFileDialog.ShowDialog();

        if (result != true)
            return null;
        else
            return saveFileDialog.FileName;
    }

    public bool EditGame(EditGameViewModel viewModel)
    {
        var editGameWin = new EditGameWindow(viewModel);
        editGameWin.ShowDialog();

        return editGameWin.DialogResult == true;
    }
}