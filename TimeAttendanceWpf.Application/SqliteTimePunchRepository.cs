using Microsoft.Data.Sqlite;
using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public sealed class SqliteTimePunchRepository : ITimePunchRepository
{
    private readonly string _connectionString;

    public SqliteTimePunchRepository(string dbFilePath)
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

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS TimePunches (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeId TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                Type INTEGER NOT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_TimePunches_Employee_Day
            ON TimePunches(EmployeeId, Timestamp);
            """;

        command.ExecuteNonQuery();
    }


    public void Add(TimePunch punch)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TimePunches (EmployeeId, Timestamp, Type)
            VALUES ($employeeId, $timestamp, $type);
            """;

        command.Parameters.AddWithValue("$employeeId", punch.EmployeeId.ToString());
        command.Parameters.AddWithValue("$timestamp", punch.Timestamp.ToString("O")); // ISO 8601
        command.Parameters.AddWithValue("$type", (int)punch.Type);

        command.ExecuteNonQuery();
    }

    public IReadOnlyList<TimePunch> GetForDay(Guid employeeId, DateTime day)
    {
        var start = day.Date;
        var end = start.AddDays(1);

        var results = new List<TimePunch>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT EmployeeId, Timestamp, Type
            FROM TimePunches
            WHERE EmployeeId = $employeeId
              AND Timestamp >= $start
              AND Timestamp < $end
            ORDER BY Timestamp ASC;
            """;

        command.Parameters.AddWithValue("$employeeId", employeeId.ToString());
        command.Parameters.AddWithValue("$start", start.ToString("O"));
        command.Parameters.AddWithValue("$end", end.ToString("O"));

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var empId = Guid.Parse(reader.GetString(0));
            var timestamp = DateTime.Parse(reader.GetString(1), null, System.Globalization.DateTimeStyles.RoundtripKind);
            var type = (PunchType)reader.GetInt32(2);

            results.Add(new TimePunch(empId, timestamp, type));
        }

        return results;
    }
}
