namespace TimeAttendanceWpf.Domain;

public sealed class OvertimeResult
{
    public double RegularHours { get; }
    public double OvertimeHours { get; }
    public double TotalHours => RegularHours + OvertimeHours;

    public OvertimeResult(double regularHours, double overtimeHours)
    {
        RegularHours = regularHours;
        OvertimeHours = overtimeHours;
    }
}
