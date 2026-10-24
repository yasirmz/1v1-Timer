using System.Windows;

namespace OneVOneTimer.Ui;

/// <summary>The settings window: Capture's look on one page. It is built again when the language changes.</summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Title = App.Text.Get("app") + " · " + App.Text.Get("settings.title");
        Icon = App.AppIcon.Window();
        FlowDirection = App.Text.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        Closed += (_, _) => vm.Detach();
    }

    void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    void OnClose(object sender, RoutedEventArgs e) => Close();
}
