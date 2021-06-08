using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public abstract class EditorFieldViewModel : BaseViewModel
    {
        protected EditorFieldViewModel(string header, string info = null)
        {
            Header = header;
            Info = info;
        }

        public string Header { get; }
        public string Info { get; }

        public abstract void Refresh();
    }
}