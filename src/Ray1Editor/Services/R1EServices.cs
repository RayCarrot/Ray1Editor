namespace Ray1Editor
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
            File = new FileManager();
        }

        public static AppViewModel App { get; }
        public static AppUIManager UI { get; }
        public static FileManager File { get; }
    }
}