namespace RayCarrot.Ray1Editor
{
    public class LoadGameLevelViewModel : BaseViewModel
    {
        public LoadGameLevelViewModel(string header, object settings)
        {
            Header = header;
            Settings = settings;
        }

        public string Header { get; }
        public object Settings { get; }
        public bool IsSelectable => Settings != null;
    }
}