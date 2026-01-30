namespace TimeAttendanceWpf.Domain;

public sealed class TimesheetWeek
{
    public Guid EmployeeId { get; }
    public DateTime WeekStart { get; } // Monday 00:00 local
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;

    public TimesheetWeek(Guid employeeId, DateTime weekStart)
    {
        EmployeeId = employeeId;
        WeekStart = weekStart.Date;
    }
}
