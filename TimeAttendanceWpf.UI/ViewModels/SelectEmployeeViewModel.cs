using TimeAttendanceWpf.Domain;
using TimeAttendanceWpf.UI.Services;

namespace TimeAttendanceWpf.UI.ViewModels;

public sealed class SelectEmployeeViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;
    private readonly SessionStore _sessionStore;

    private readonly List<Employee> _employees = new()
    {
        new Employee(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Dawson",
            EmployeeRole.Employee),

        new Employee(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Alex",
            EmployeeRole.Manager),
    };

    public List<string> EmployeeNames =>
        _employees.Select(e => $"{e.Name} ({e.Role})").ToList();

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            OnPropertyChanged();
            ContinueCommand?.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand ContinueCommand { get; }

    public SelectEmployeeViewModel(
        NavigationStore navigationStore,
        SessionStore sessionStore)
    {
        _navigationStore = navigationStore;
        _sessionStore = sessionStore;

        ContinueCommand = new RelayCommand(Continue, CanContinue);
        SelectedIndex = 0;
    }

    private bool CanContinue() =>
        SelectedIndex >= 0 && SelectedIndex < _employees.Count;

    private void Continue()
    {
        var selectedEmployee = _employees[SelectedIndex];
        _sessionStore.CurrentEmployee = selectedEmployee;

        if (selectedEmployee.Role == EmployeeRole.Employee)
        {
            _navigationStore.CurrentViewModel =
                new EmployeeDashboardViewModel(_navigationStore, _sessionStore);
        }
        else
        {
            _navigationStore.CurrentViewModel =
                new ManagerDashboardViewModel(_navigationStore, _sessionStore);
        }
    }
}
