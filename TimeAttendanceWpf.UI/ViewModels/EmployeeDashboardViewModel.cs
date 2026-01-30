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

    private bool _isClockedIn;
    public bool IsClockedIn
    {
        get => _isClockedIn;
        private set
        {
            _isClockedIn = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand ClockInCommand { get; }
    public RelayCommand ClockOutCommand { get; }
    public RelayCommand BackCommand { get; }

    public EmployeeDashboardViewModel(
        NavigationStore navigationStore,
        SessionStore sessionStore)
    {
        _navigationStore = navigationStore;
        _sessionStore = sessionStore;

        _timeClockService =
            (TimeClockService)System.Windows.Application.Current.Resources["TimeClockService"];

        ClockInCommand = new RelayCommand(ClockIn, CanClockIn);
        ClockOutCommand = new RelayCommand(ClockOut, CanClockOut);
        BackCommand = new RelayCommand(Back);

        RefreshPunchesAndState();
    }

    private bool CanClockIn() => _sessionStore.CurrentEmployee != null && !IsClockedIn;
    private bool CanClockOut() => _sessionStore.CurrentEmployee != null && IsClockedIn;

    private void ClockIn()
    {
        try
        {
            var emp = RequireEmployee();
            _timeClockService.ClockIn(emp.Id, DateTime.Now);
            RefreshPunchesAndState();
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
            RefreshPunchesAndState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clock Out Failed");
        }
    }

    private void RefreshPunchesAndState()
    {
        PunchLines.Clear();

        var emp = _sessionStore.CurrentEmployee;
        if (emp is null)
        {
            IsClockedIn = false;
            ClockInCommand.RaiseCanExecuteChanged();
            ClockOutCommand.RaiseCanExecuteChanged();
            return;
        }

        var punches = _timeClockService.GetTodayPunches(emp.Id);

        if (punches.Count == 0)
        {
            IsClockedIn = false;
            PunchLines.Add("No punches today.");
        }
        else
        {
            foreach (var punch in punches)
                PunchLines.Add($"{punch.Timestamp:t} - {punch.Type}");

            IsClockedIn = punches.Last().Type == PunchType.In;
        }

        ClockInCommand.RaiseCanExecuteChanged();
        ClockOutCommand.RaiseCanExecuteChanged();
    }

    private Employee RequireEmployee()
        => _sessionStore.CurrentEmployee ?? throw new InvalidOperationException("No employee selected.");

    private void Back()
    {
        _navigationStore.CurrentViewModel =
            new SelectEmployeeViewModel(_navigationStore, _sessionStore);
    }
}
