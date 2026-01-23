using System.Windows;
using TimeAttendanceWpf.UI.Services;
using TimeAttendanceWpf.UI.ViewModels;

namespace TimeAttendanceWpf.UI;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var navigationStore = new NavigationStore();

        navigationStore.CurrentViewModel =
            new SelectEmployeeViewModel();

        var mainViewModel =
            new MainViewModel(navigationStore);

        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };

        mainWindow.Show();
    }
}
