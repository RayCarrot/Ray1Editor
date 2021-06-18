namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Services used in the Ray1Editor
    /// </summary>
    public static class R1EServices
    {
        static R1EServices()
        {
            // IDEA: Move to dependency injection?
            App = new AppViewModel();
            UI = new AppUIManager();
            Updater = new R1EUpdateManager();
            File = new FileManager();
        }

        public static AppViewModel App { get; }
        public static AppUIManager UI { get; }
        public static UpdaterManager Updater { get; }
        public static FileManager File { get; }
    }
}