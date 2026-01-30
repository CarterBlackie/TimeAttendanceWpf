namespace TimeAttendanceWpf.Application;

public static class TimeRounding
{
    // Nearest N minutes (e.g., 15)
    public static DateTime RoundToNearestMinutes(DateTime dt, int minutes)
    {
        if (minutes <= 0) return dt;

        var ticks = TimeSpan.FromMinutes(minutes).Ticks;
        var rounded = (long)Math.Round(dt.Ticks / (double)ticks) * ticks;
        return new DateTime(rounded, dt.Kind);
    }
}
