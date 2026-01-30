using System;
using System.IO;
using TimeAttendanceWpf.Application;
using TimeAttendanceWpf.Domain;
using Xunit;

namespace TimeAttendanceWpf.Tests;

public sealed class TimeClockServiceTests
{
    private static string NewTempDbPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "TimeAttendanceWpfTests");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"{Guid.NewGuid():N}.db");
    }

    [Fact]
    public void ClockIn_Adds_In_Punch()
    {
        var dbPath = NewTempDbPath();
        try
        {
            ITimePunchRepository repo = new SqliteTimePunchRepository(dbPath);
            var service = new TimeClockService(repo);

            var employeeId = Guid.NewGuid();
            var now = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 9, 0, 0), DateTimeKind.Utc);

            service.ClockIn(employeeId, now);

            var punches = repo.GetForDay(employeeId, now);
            Assert.Single(punches);
            Assert.Equal(PunchType.In, punches[0].Type);
            Assert.Equal(now, punches[0].Timestamp);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void ClockIn_Twice_Throws()
    {
        var dbPath = NewTempDbPath();
        try
        {
            ITimePunchRepository repo = new SqliteTimePunchRepository(dbPath);
            var service = new TimeClockService(repo);

            var employeeId = Guid.NewGuid();
            var t1 = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 9, 0, 0), DateTimeKind.Utc);
            var t2 = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 9, 5, 0), DateTimeKind.Utc);

            service.ClockIn(employeeId, t1);

            var ex = Assert.Throws<InvalidOperationException>(() => service.ClockIn(employeeId, t2));
            Assert.Contains("already clocked in", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void ClockOut_Without_ClockIn_Throws()
    {
        var dbPath = NewTempDbPath();
        try
        {
            ITimePunchRepository repo = new SqliteTimePunchRepository(dbPath);
            var service = new TimeClockService(repo);

            var employeeId = Guid.NewGuid();
            var now = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 17, 0, 0), DateTimeKind.Utc);

            var ex = Assert.Throws<InvalidOperationException>(() => service.ClockOut(employeeId, now));
            Assert.Contains("clock in", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void ClockOut_After_ClockIn_Adds_Out_Punch()
    {
        var dbPath = NewTempDbPath();
        try
        {
            ITimePunchRepository repo = new SqliteTimePunchRepository(dbPath);
            var service = new TimeClockService(repo);

            var employeeId = Guid.NewGuid();
            var inTime = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 9, 0, 0), DateTimeKind.Utc);
            var outTime = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 17, 0, 0), DateTimeKind.Utc);

            service.ClockIn(employeeId, inTime);
            service.ClockOut(employeeId, outTime);

            var punches = repo.GetForDay(employeeId, inTime);

            Assert.Equal(2, punches.Count);
            Assert.Equal(PunchType.In, punches[0].Type);
            Assert.Equal(PunchType.Out, punches[1].Type);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void ClockOut_Before_ClockIn_Time_Throws()
    {
        var dbPath = NewTempDbPath();
        try
        {
            ITimePunchRepository repo = new SqliteTimePunchRepository(dbPath);
            var service = new TimeClockService(repo);

            var employeeId = Guid.NewGuid();
            var inTime = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 9, 0, 0), DateTimeKind.Utc);
            var outTime = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 8, 59, 0), DateTimeKind.Utc);

            service.ClockIn(employeeId, inTime);

            var ex = Assert.Throws<InvalidOperationException>(() => service.ClockOut(employeeId, outTime));
            Assert.Contains("before clock-in", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}
