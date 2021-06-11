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
            get => Data.DarkTheme;
            set
            {
                Data.DarkTheme = value;
                App.Current.UpdateTheme();
            }
        }

        public bool SyncTheme
        {
            get => Data.SyncTheme;
            set
            {
                Data.SyncTheme = value;
                App.Current.UpdateTheme();
            }
        }

        public bool EnableSerializerLog
        {
            get => Data.EnableSerializerLog;
            set => Data.EnableSerializerLog = value;
        }
    }
}