using TimeAttendanceWpf.UI.Services;

namespace TimeAttendanceWpf.UI.ViewModels;

public sealed class ManagerDashboardViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;
    private readonly SessionStore _sessionStore;

    public string Title => $"Manager: {_sessionStore.CurrentEmployee?.Name}";

    public RelayCommand BackCommand { get; }

    public ManagerDashboardViewModel(NavigationStore navigationStore, SessionStore sessionStore)
    {
        _navigationStore = navigationStore;
        _sessionStore = sessionStore;

        BackCommand = new RelayCommand(() =>
            _navigationStore.CurrentViewModel = new SelectEmployeeViewModel(_navigationStore, _sessionStore));
    }
}
