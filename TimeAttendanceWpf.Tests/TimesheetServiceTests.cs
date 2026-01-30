using TimeAttendanceWpf.Application;
using TimeAttendanceWpf.Domain;
using Xunit;

public class TimesheetServiceTests
{
    [Fact]
    public void Timesheet_Status_Flows_Draft_To_Approved()
    {
        var dbPath = Path.GetTempFileName();

        var punchRepo = new SqliteTimePunchRepository(dbPath);
        var timesheetRepo = new SqliteTimesheetRepository(dbPath);
        var service = new TimesheetService(punchRepo, timesheetRepo);

        var empId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 27);

        Assert.Equal(TimesheetStatus.Draft, service.GetStatus(empId, date));

        service.Submit(empId, date);
        Assert.Equal(TimesheetStatus.Submitted, service.GetStatus(empId, date));

        service.Approve(empId, date);
        Assert.Equal(TimesheetStatus.Approved, service.GetStatus(empId, date));
    }
}
