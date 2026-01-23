using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public sealed class TimeClockService
{
    private readonly ITimePunchRepository _repository;

    public TimeClockService(ITimePunchRepository repository)
    {
        _repository = repository;
    }

    public void ClockIn(Guid employeeId, DateTime now)
    {
        var punches = _repository.GetForDay(employeeId, now);

        if (punches.LastOrDefault()?.Type == PunchType.In)
            throw new InvalidOperationException("Employee is already clocked in.");

        _repository.Add(new TimePunch(employeeId, now, PunchType.In));
    }

    public void ClockOut(Guid employeeId, DateTime now)
    {
        var punches = _repository.GetForDay(employeeId, now);

        if (punches.Count == 0 || punches.Last().Type != PunchType.In)
            throw new InvalidOperationException("Employee must clock in before clocking out.");

        var lastIn = punches.Last(p => p.Type == PunchType.In);

        if (now < lastIn.Timestamp)
            throw new InvalidOperationException("Clock-out time cannot be before clock-in.");

        _repository.Add(new TimePunch(employeeId, now, PunchType.Out));
    }
}
