using Microsoft.Data.Sqlite;
using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public sealed class SqliteTimesheetRepository : ITimesheetRepository
{
    private readonly string _connectionString;

    public SqliteTimesheetRepository(string dbFilePath)
    {
        var directory = Path.GetDirectoryName(dbFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        _connectionString = $"Data Source={dbFilePath};Pooling=False";
        Initialize();
    }

    private void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Timesheets (
                EmployeeId TEXT NOT NULL,
                WeekStart TEXT NOT NULL,
                Status INTEGER NOT NULL,
                ManagerNote TEXT NULL,
                UpdatedAt TEXT NOT NULL,
                PRIMARY KEY (EmployeeId, WeekStart)
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public TimesheetStatus GetStatus(Guid employeeId, DateTime weekStart)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT Status
            FROM Timesheets
            WHERE EmployeeId = $emp AND WeekStart = $week;
            """;

        cmd.Parameters.AddWithValue("$emp", employeeId.ToString());
        cmd.Parameters.AddWithValue("$week", weekStart.Date.ToString("O"));

        var result = cmd.ExecuteScalar();
        if (result is null) return TimesheetStatus.Draft;

        return (TimesheetStatus)Convert.ToInt32(result);
    }

    public string? GetManagerNote(Guid employeeId, DateTime weekStart)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT ManagerNote
            FROM Timesheets
            WHERE EmployeeId = $emp AND WeekStart = $week;
            """;

        cmd.Parameters.AddWithValue("$emp", employeeId.ToString());
        cmd.Parameters.AddWithValue("$week", weekStart.Date.ToString("O"));

        var result = cmd.ExecuteScalar();
        return result is null ? null : Convert.ToString(result);
    }

    public void SetStatus(Guid employeeId, DateTime weekStart, TimesheetStatus status, string? managerNote = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO Timesheets (EmployeeId, WeekStart, Status, ManagerNote, UpdatedAt)
            VALUES ($emp, $week, $status, $note, $updated)
            ON CONFLICT(EmployeeId, WeekStart)
            DO UPDATE SET
              Status = excluded.Status,
              ManagerNote = excluded.ManagerNote,
              UpdatedAt = excluded.UpdatedAt;
            """;

        cmd.Parameters.AddWithValue("$emp", employeeId.ToString());
        cmd.Parameters.AddWithValue("$week", weekStart.Date.ToString("O"));
        cmd.Parameters.AddWithValue("$status", (int)status);
        cmd.Parameters.AddWithValue("$note", (object?)managerNote ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$updated", DateTime.UtcNow.ToString("O"));

        cmd.ExecuteNonQuery();
    }
}
