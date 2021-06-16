using System.Collections.ObjectModel;
using System.Windows.Media;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class DialogMessageViewModel : BaseViewModel
    {
        /// <summary>
        /// The title to use for the user input action
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// The message text
        /// </summary>
        public string MessageText { get; init; }

        /// <summary>
        /// The message type
        /// </summary>
        public DialogMessageType MessageType { get; init; }

        /// <summary>
        /// The dialog actions
        /// </summary>
        public ObservableCollection<DialogMessageActionViewModel> DialogActions { get; init; }

        /// <summary>
        /// The default action result
        /// </summary>
        public object DefaultActionResult { get; init; }

        /// <summary>
        /// The dialog image source
        /// </summary>
        public ImageSource DialogImageSource { get; init; }
    }
}