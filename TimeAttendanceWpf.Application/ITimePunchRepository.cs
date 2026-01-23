using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public interface ITimePunchRepository
{
    void Add(TimePunch punch);
    IReadOnlyList<TimePunch> GetForDay(Guid employeeId, DateTime day);
}
