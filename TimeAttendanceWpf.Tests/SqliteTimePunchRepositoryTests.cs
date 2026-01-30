using System;
using System.IO;
using TimeAttendanceWpf.Application;
using TimeAttendanceWpf.Domain;
using Xunit;

namespace TimeAttendanceWpf.Tests;

public sealed class SqliteTimePunchRepositoryTests
{
    private static string NewTempDbPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "TimeAttendanceWpfTests");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"{Guid.NewGuid():N}.db");
    }

    [Fact]
    public void Repository_Persists_Punches_Across_Instances()
    {
        var dbPath = NewTempDbPath();
        try
        {
            var employeeId = Guid.NewGuid();
            var t1 = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 9, 0, 0), DateTimeKind.Utc);

            // First instance writes
            var repo1 = new SqliteTimePunchRepository(dbPath);
            repo1.Add(new TimePunch(employeeId, t1, PunchType.In));

            // Second instance reads (proves persistence)
            var repo2 = new SqliteTimePunchRepository(dbPath);
            var punches = repo2.GetForDay(employeeId, t1);

            Assert.Single(punches);
            Assert.Equal(PunchType.In, punches[0].Type);
            Assert.Equal(t1, punches[0].Timestamp);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void Repository_Returns_Punches_In_Time_Order()
    {
        var dbPath = NewTempDbPath();
        try
        {
            var employeeId = Guid.NewGuid();
            var day = DateTime.SpecifyKind(new DateTime(2026, 1, 30, 0, 0, 0), DateTimeKind.Utc);

            var t1 = day.AddHours(9);
            var t2 = day.AddHours(17);

            var repo = new SqliteTimePunchRepository(dbPath);
            repo.Add(new TimePunch(employeeId, t2, PunchType.Out));
            repo.Add(new TimePunch(employeeId, t1, PunchType.In));

            var punches = repo.GetForDay(employeeId, day);

            Assert.Equal(2, punches.Count);
            Assert.Equal(t1, punches[0].Timestamp);
            Assert.Equal(t2, punches[1].Timestamp);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}
