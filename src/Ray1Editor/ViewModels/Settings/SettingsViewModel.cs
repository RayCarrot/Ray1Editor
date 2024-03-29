﻿using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;

namespace Ray1Editor;

public class SettingsViewModel : BaseViewModel
{
    #region Constructor

    public SettingsViewModel()
    {
        Data = R1EServices.App.UserData;
        EditorColorProfiles = new ObservableCollection<EditorColorProfileViewModel>(EditorColorProfileViewModel.GetViewModels);
        OpenSerializerLogCommand = new RelayCommand(OpenSerializerLog);
        BrowsemkpsxisoCommand = new RelayCommand(Browsemkpsxiso);
    }

    #endregion

    #region Commands

    public ICommand OpenSerializerLogCommand { get; }
    public ICommand BrowsemkpsxisoCommand { get; }

    #endregion

    #region Public Properties

    public AppUserData Data { get; }

    public bool DarkTheme
    {
        get => Data.Theme_Dark;
        set
        {
            Data.Theme_Dark = value;
            App.Current.UpdateTheme();
        }
    }

    public bool SyncTheme
    {
        get => Data.Theme_Sync;
        set
        {
            Data.Theme_Sync = value;
            App.Current.UpdateTheme();
        }
    }

    public bool PauseWhenInactive
    {
        get => Data.Editor_PauseWhenInactive;
        set => Data.Editor_PauseWhenInactive = value;
    }

    public bool EnableSerializerLog
    {
        get => Data.Serializer_EnableLog;
        set => Data.Serializer_EnableLog = value;
    }

    public bool CreateBackupOnWrite
    {
        get => Data.Serializer_CreateBackupOnWrite;
        set => Data.Serializer_CreateBackupOnWrite = value;
    }

    public bool UI_ShowDebugInfo
    {
        get => Data.UI_ShowDebugInfo;
        set => Data.UI_ShowDebugInfo = value;
    }

    public string PS1_mkpsxisoPath
    {
        get => Data.PS1_mkpsxisoPath;
        set => Data.PS1_mkpsxisoPath = value;
    }

    public EditorColorProfileViewModel SelectedColorProfile
    {
        get => EditorColorProfiles.FirstOrDefault(x => x.ID == Data.Theme_EditorColors) ?? EditorColorProfiles.First();
        set => Data.Theme_EditorColors = value.ID;
    }

    public ObservableCollection<EditorColorProfileViewModel> EditorColorProfiles { get; }

    #endregion

    #region Public Methods

    public void OpenSerializerLog()
    {
        var file = R1EServices.App.Path_SerializerLogFile;

        if (File.Exists(file))
            R1EServices.File.LaunchFile(file);
        else
            R1EServices.UI.DisplayMessage("No serializer log file has been created", "File does not exist", DialogMessageType.Information);
    }

    public void Browsemkpsxiso()
    {
        // Create the dialog
        OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Title = "Select the file path",
            FileName = PS1_mkpsxisoPath,
            CheckFileExists = true,
        };

        // Show the dialog and get the result
        if (openFileDialog.ShowDialog() == true)
            PS1_mkpsxisoPath = openFileDialog.FileName;
    }

    #endregion
}