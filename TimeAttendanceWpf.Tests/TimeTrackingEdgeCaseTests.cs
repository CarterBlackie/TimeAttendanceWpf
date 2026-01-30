using System;
using System.IO;
using TimeAttendanceWpf.Application;
using TimeAttendanceWpf.Domain;
using Xunit;

namespace TimeAttendanceWpf.Tests;

public sealed class TimeTrackingEdgeCaseTests
{
    private static string NewDb()
        => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

    [Fact]
    public void ClockIn_Out_Then_In_Again_Is_Allowed()
    {
        var db = NewDb();
        var repo = new SqliteTimePunchRepository(db);
        var service = new TimeClockService(repo);

        var emp = Guid.NewGuid();
        var day = new DateTime(2026, 2, 3);

        service.ClockIn(emp, day.AddHours(9));
        service.ClockOut(emp, day.AddHours(12));
        service.ClockIn(emp, day.AddHours(13));

        var punches = repo.GetForDay(emp, day);

        Assert.Equal(3, punches.Count);
        Assert.Equal(PunchType.In, punches[2].Type);
    }

    [Fact]
    public void Punches_Do_Not_Leak_Into_Other_Days()
    {
        var db = NewDb();
        var repo = new SqliteTimePunchRepository(db);

        var emp = Guid.NewGuid();

        repo.Add(new TimePunch(emp, new DateTime(2026, 2, 3, 23, 0, 0), PunchType.In));
        repo.Add(new TimePunch(emp, new DateTime(2026, 2, 4, 1, 0, 0), PunchType.Out));

        var day1 = repo.GetForDay(emp, new DateTime(2026, 2, 3));
        var day2 = repo.GetForDay(emp, new DateTime(2026, 2, 4));

        Assert.Single(day1);
        Assert.Single(day2);
    }

    [Fact]
    public void Rounding_To_15_Minutes_Works_Correctly()
    {
        var dt = new DateTime(2026, 2, 3, 9, 8, 0);
        var rounded = TimeRounding.RoundToNearestMinutes(dt, 15);

        Assert.Equal(new DateTime(2026, 2, 3, 9, 15, 0), rounded);
    }

    [Fact]
    public void Weekly_Overtime_Is_Calculated_Correctly()
    {
        var shifts = new[]
        {
            new WorkShift(new DateTime(2026,2,3,9,0,0), new DateTime(2026,2,3,18,0,0)), // 9h
            new WorkShift(new DateTime(2026,2,4,9,0,0), new DateTime(2026,2,4,18,0,0)), // 9h
            new WorkShift(new DateTime(2026,2,5,9,0,0), new DateTime(2026,2,5,18,0,0)), // 9h
            new WorkShift(new DateTime(2026,2,6,9,0,0), new DateTime(2026,2,6,18,0,0)), // 9h
            new WorkShift(new DateTime(2026,2,7,9,0,0), new DateTime(2026,2,7,18,0,0)), // 9h
        };

        var calc = new TimesheetCalculator();
        var result = calc.CalculateWeeklyOvertime(shifts, 40);

        Assert.Equal(40, result.RegularHours);
        Assert.Equal(5, result.OvertimeHours);
    }
}
