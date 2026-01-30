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
    private readonly TimesheetService _timesheetService;

    private DateTime _weekAnchorDate = DateTime.Today;

    private const int RoundingMinutes = 15;
    private const double WeeklyOvertimeThreshold = 40;

    public string Title => $"Employee: {_sessionStore.CurrentEmployee?.Name}";

    public string WeekLabel => $"Week of {WeekStart:yyyy-MM-dd}";
    public DateTime WeekStart { get; private set; }

    private TimesheetStatus _status;
    public TimesheetStatus Status
    {
        get => _status;
        private set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
            SubmitTimesheetCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusText => $"Status: {Status}";

    private string _managerNote = "";
    public string ManagerNote
    {
        get => _managerNote;
        private set
        {
            _managerNote = value;
            OnPropertyChanged();
        }
    }

    private double _regularHours;
    public double RegularHours
    {
        get => _regularHours;
        private set
        {
            _regularHours = value;
            OnPropertyChanged();
        }
    }

    private double _overtimeHours;
    public double OvertimeHours
    {
        get => _overtimeHours;
        private set
        {
            _overtimeHours = value;
            OnPropertyChanged();
        }
    }

    private double _totalHours;
    public double TotalHours
    {
        get => _totalHours;
        private set
        {
            _totalHours = value;
            OnPropertyChanged();
        }
    }

    private bool _isClockedIn;
    public bool IsClockedIn
    {
        get => _isClockedIn;
        private set
        {
            _isClockedIn = value;
            OnPropertyChanged();
            ClockInCommand.RaiseCanExecuteChanged();
            ClockOutCommand.RaiseCanExecuteChanged();
        }
    }

    public ObservableCollection<string> PunchLines { get; } = new();
    public ObservableCollection<string> ShiftLines { get; } = new();

    public RelayCommand ClockInCommand { get; }
    public RelayCommand ClockOutCommand { get; }
    public RelayCommand SubmitTimesheetCommand { get; }
    public RelayCommand PrevWeekCommand { get; }
    public RelayCommand NextWeekCommand { get; }
    public RelayCommand BackCommand { get; }

    public EmployeeDashboardViewModel(NavigationStore navigationStore, SessionStore sessionStore)
    {
        _navigationStore = navigationStore;
        _sessionStore = sessionStore;

        _timeClockService =
            (TimeClockService)System.Windows.Application.Current.Resources["TimeClockService"];

        _timesheetService =
            (TimesheetService)System.Windows.Application.Current.Resources["TimesheetService"];

        ClockInCommand = new RelayCommand(ClockIn, CanClockIn);
        ClockOutCommand = new RelayCommand(ClockOut, CanClockOut);

        SubmitTimesheetCommand = new RelayCommand(SubmitTimesheet, CanSubmitTimesheet);

        PrevWeekCommand = new RelayCommand(() => ChangeWeek(-7));
        NextWeekCommand = new RelayCommand(() => ChangeWeek(+7));

        BackCommand = new RelayCommand(Back);

        RefreshAll();
    }

    private bool CanClockIn() => _sessionStore.CurrentEmployee != null && !IsClockedIn;
    private bool CanClockOut() => _sessionStore.CurrentEmployee != null && IsClockedIn;

    private bool CanSubmitTimesheet()
    {
        // Allow submit when Draft or Rejected
        return _sessionStore.CurrentEmployee != null &&
               (Status == TimesheetStatus.Draft || Status == TimesheetStatus.Rejected);
    }

    private void ClockIn()
    {
        try
        {
            var emp = RequireEmployee();
            _timeClockService.ClockIn(emp.Id, DateTime.Now);
            RefreshAll();
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
            RefreshAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clock Out Failed");
        }
    }

    private void SubmitTimesheet()
    {
        try
        {
            var emp = RequireEmployee();
            _timesheetService.Submit(emp.Id, _weekAnchorDate);
            RefreshTimesheet();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Submit Failed");
        }
    }

    private void ChangeWeek(int days)
    {
        _weekAnchorDate = _weekAnchorDate.AddDays(days);
        RefreshAll();
        OnPropertyChanged(nameof(WeekLabel));
    }

    private void RefreshAll()
    {
        RefreshPunchesAndState();
        RefreshTimesheet();

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(WeekLabel));
    }

    private void RefreshPunchesAndState()
    {
        PunchLines.Clear();

        var emp = _sessionStore.CurrentEmployee;
        if (emp is null)
        {
            IsClockedIn = false;
            PunchLines.Add("No employee selected.");
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
    }

    private void RefreshTimesheet()
    {
        ShiftLines.Clear();

        var emp = _sessionStore.CurrentEmployee;
        if (emp is null)
        {
            WeekStart = DateTime.Today;
            Status = TimesheetStatus.Draft;
            ManagerNote = "";
            RegularHours = 0;
            OvertimeHours = 0;
            TotalHours = 0;
            return;
        }

        var (shifts, overtime, status, note) =
            _timesheetService.GetWeekSummary(
                emp.Id,
                _weekAnchorDate,
                roundingMinutes: RoundingMinutes,
                weeklyThreshold: WeeklyOvertimeThreshold);

        WeekStart = _timesheetService.GetWeekStart(_weekAnchorDate);
        Status = status;

        ManagerNote = string.IsNullOrWhiteSpace(note) ? "" : $"Manager Note: {note}";

        RegularHours = overtime.RegularHours;
        OvertimeHours = overtime.OvertimeHours;
        TotalHours = overtime.TotalHours;

        if (shifts.Count == 0)
        {
            ShiftLines.Add("No shifts in this week.");
        }
        else
        {
            foreach (var s in shifts)
            {
                ShiftLines.Add($"{s.Start:ddd HH:mm} - {s.End:ddd HH:mm} ({Math.Round(s.Hours, 2)}h)");
            }
        }

        SubmitTimesheetCommand.RaiseCanExecuteChanged();
    }

    private Employee RequireEmployee()
        => _sessionStore.CurrentEmployee ?? throw new InvalidOperationException("No employee selected.");

    private void Back()
    {
        _navigationStore.CurrentViewModel =
            new SelectEmployeeViewModel(_navigationStore, _sessionStore);
    }
}
