using System.Windows.Input;
using Microsoft.Win32;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Data = AppViewModel.Instance.UserData;
            BrowsemkpsxisoCommand = new RelayCommand(Browsemkpsxiso);
        }

        public ICommand BrowsemkpsxisoCommand { get; }

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

        public string PS1_mkpsxisoPath
        {
            get => Data.PS1_mkpsxisoPath;
            set => Data.PS1_mkpsxisoPath = value;
        }

        public bool Update_CheckOnLaunch
        {
            get => Data.Update_CheckOnLaunch;
            set => Data.Update_CheckOnLaunch = value;
        }

        public bool Update_GetBeta
        {
            get => Data.Update_GetBeta;
            set => Data.Update_GetBeta = value;
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
    }
}