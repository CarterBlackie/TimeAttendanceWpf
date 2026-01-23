using System;
using System.Windows;
using TimeAttendanceWpf.Application;
using TimeAttendanceWpf.UI.Services;
using TimeAttendanceWpf.UI.ViewModels;

namespace TimeAttendanceWpf.UI;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            // If something throws later on the UI thread, show it.
            DispatcherUnhandledException += (_, args) =>
            {
                MessageBox.Show(args.Exception.ToString(), "Unhandled UI Exception");
                args.Handled = true;
                Shutdown(-1);
            };

            var navigationStore = new NavigationStore();
            var sessionStore = new SessionStore();

            var punchRepo = new InMemoryTimePunchRepository();
            var timeClockService = new TimeClockService(punchRepo);

            Resources["TimeClockService"] = timeClockService;

            navigationStore.CurrentViewModel =
                new SelectEmployeeViewModel(navigationStore, sessionStore);

            var mainViewModel = new MainViewModel(navigationStore);

            var window = new MainWindow
            {
                DataContext = mainViewModel
            };

            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Startup Failed");
            Shutdown(-1);
        }
    }
}
