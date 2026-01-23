namespace TimeAttendanceWpf.Domain;

public sealed class WorkShift
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public double Hours => (End - Start).TotalHours;

    public WorkShift(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("Shift end must be after start.");

        Start = start;
        End = end;
    }
}
