using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public interface ITimesheetRepository
{
    TimesheetStatus GetStatus(Guid employeeId, DateTime weekStart);
    void SetStatus(Guid employeeId, DateTime weekStart, TimesheetStatus status, string? managerNote = null);
    string? GetManagerNote(Guid employeeId, DateTime weekStart);
}
