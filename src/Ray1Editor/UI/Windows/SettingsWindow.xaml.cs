namespace Ray1Editor;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : BaseWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();

        // Save settings when closing
        Closed += (_, _) => R1EServices.App.SaveAppUserData();
    }
}