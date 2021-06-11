using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Data = AppViewModel.Instance.UserData;
        }

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
    }
}