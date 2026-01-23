namespace TimeAttendanceWpf.Domain;

public enum PunchType
{
    In,
    Out
}

public sealed class TimePunch
{
    public Guid EmployeeId { get; }
    public DateTime Timestamp { get; }
    public PunchType Type { get; }

    public TimePunch(Guid employeeId, DateTime timestamp, PunchType type)
    {
        EmployeeId = employeeId;
        Timestamp = timestamp;
        Type = type;
    }
}
