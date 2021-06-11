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

        public bool EnableSerializerLog
        {
            get => Data.EnableSerializerLog;
            set => Data.EnableSerializerLog = value;
        }
    }
}