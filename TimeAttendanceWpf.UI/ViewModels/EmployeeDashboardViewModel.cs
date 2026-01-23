using System.Collections.ObjectModel;
using System.Windows;
using TimeAttendanceWpf.Application;
using TimeAttendanceWpf.Domain;
using TimeAttendanceWpf.UI.Services;

namespace TimeAttendanceWpf.UI.ViewModels;

public sealed class EmployeeDashboardViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;
    private readonly SessionStore _sessionStore;
    private readonly TimeClockService _timeClockService;

    public string Title => $"Employee: {_sessionStore.CurrentEmployee?.Name}";

    public ObservableCollection<string> PunchLines { get; } = new();

    public RelayCommand ClockInCommand { get; }
    public RelayCommand ClockOutCommand { get; }
    public RelayCommand BackCommand { get; }

    public EmployeeDashboardViewModel(NavigationStore navigationStore, SessionStore sessionStore)
    {
        _navigationStore = navigationStore;
        _sessionStore = sessionStore;

        _timeClockService = (TimeClockService)System.Windows.Application.Current.Resources["TimeClockService"];

        ClockInCommand = new RelayCommand(ClockIn);
        ClockOutCommand = new RelayCommand(ClockOut);
        BackCommand = new RelayCommand(Back);

        RefreshPunches();
    }

    private void ClockIn()
    {
        try
        {
            var emp = RequireEmployee();
            _timeClockService.ClockIn(emp.Id, DateTime.Now);
            RefreshPunches();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clock In Failed");
        }
    }

    private void ClockOut()
    {
        try
        {
            var emp = RequireEmployee();
            _timeClockService.ClockOut(emp.Id, DateTime.Now);
            RefreshPunches();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clock Out Failed");
        }
    }

    private void RefreshPunches()
    {
        PunchLines.Clear();

        var emp = _sessionStore.CurrentEmployee;
        if (emp is null) return;

        PunchLines.Add("Try Clock In, then Clock Out.");
        PunchLines.Add("Next step: show real punch list from repository.");
    }

    private Employee RequireEmployee()
        => _sessionStore.CurrentEmployee ?? throw new InvalidOperationException("No employee selected.");

    private void Back()
    {
        _navigationStore.CurrentViewModel = new SelectEmployeeViewModel(_navigationStore, _sessionStore);
    }
}
