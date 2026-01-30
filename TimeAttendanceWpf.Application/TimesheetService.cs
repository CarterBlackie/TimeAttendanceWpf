using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public sealed class TimesheetService
{
    private readonly ITimePunchRepository _punchRepo;
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly TimesheetCalculator _calc = new();

    public TimesheetService(ITimePunchRepository punchRepo, ITimesheetRepository timesheetRepo)
    {
        _punchRepo = punchRepo;
        _timesheetRepo = timesheetRepo;
    }

    public DateTime GetWeekStart(DateTime date)
    {
        // Monday-based week
        var d = date.Date;
        int diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }

    public (IReadOnlyList<WorkShift> shifts, OvertimeResult overtime, TimesheetStatus status, string? note)
        GetWeekSummary(Guid employeeId, DateTime anyDateInWeek, int roundingMinutes = 15, double weeklyThreshold = 40)
    {
        var weekStart = GetWeekStart(anyDateInWeek);
        var weekEnd = weekStart.AddDays(7);

        var punches = _punchRepo.GetForRange(employeeId, weekStart, weekEnd);
        var shifts = _calc.BuildShifts(punches, roundingMinutes);
        var overtime = _calc.CalculateWeeklyOvertime(shifts, weeklyThreshold);

        var status = _timesheetRepo.GetStatus(employeeId, weekStart);
        var note = _timesheetRepo.GetManagerNote(employeeId, weekStart);

        return (shifts, overtime, status, note);
    }

    public TimesheetStatus GetStatus(Guid employeeId, DateTime anyDateInWeek)
    {
        var weekStart = GetWeekStart(anyDateInWeek);
        return _timesheetRepo.GetStatus(employeeId, weekStart);
    }

    public void Submit(Guid employeeId, DateTime anyDateInWeek)
    {
        var weekStart = GetWeekStart(anyDateInWeek);
        _timesheetRepo.SetStatus(employeeId, weekStart, TimesheetStatus.Submitted);
    }

    public void Approve(Guid employeeId, DateTime anyDateInWeek, string? note = null)
    {
        var weekStart = GetWeekStart(anyDateInWeek);
        _timesheetRepo.SetStatus(employeeId, weekStart, TimesheetStatus.Approved, note);
    }

    public void Reject(Guid employeeId, DateTime anyDateInWeek, string note)
    {
        var weekStart = GetWeekStart(anyDateInWeek);
        _timesheetRepo.SetStatus(employeeId, weekStart, TimesheetStatus.Rejected, note);
    }
}
