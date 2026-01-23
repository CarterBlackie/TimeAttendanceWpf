using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public sealed class InMemoryTimePunchRepository : ITimePunchRepository
{
    private readonly List<TimePunch> _punches = new();

    public void Add(TimePunch punch)
    {
        _punches.Add(punch);
    }

    public IReadOnlyList<TimePunch> GetForDay(Guid employeeId, DateTime day)
    {
        var start = day.Date;
        var end = start.AddDays(1);

        return _punches
            .Where(p =>
                p.EmployeeId == employeeId &&
                p.Timestamp >= start &&
                p.Timestamp < end)
            .OrderBy(p => p.Timestamp)
            .ToList();
    }
}
