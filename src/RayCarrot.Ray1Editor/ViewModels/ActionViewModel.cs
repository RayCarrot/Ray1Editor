using System;
using System.Windows.Input;

namespace RayCarrot.Ray1Editor
{
    public class ActionViewModel : BaseViewModel
    {
        public ActionViewModel(string header, Action action)
        {
            Header = header;
            Action = action;
            ActionCommand = new RelayCommand(Action);
        }

        public ICommand ActionCommand { get; }

        public string Header { get; }
        public Action Action { get; }
    }
}