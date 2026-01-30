using TimeAttendanceWpf.Domain;

namespace TimeAttendanceWpf.Application;

public sealed class TimesheetCalculator
{
    // Builds shifts from punches IN/OUT pairs (same-day or across day is fine for MVP)
    public IReadOnlyList<WorkShift> BuildShifts(IReadOnlyList<TimePunch> punches, int roundingMinutes)
    {
        var shifts = new List<WorkShift>();

        TimePunch? openIn = null;

        foreach (var p in punches.OrderBy(p => p.Timestamp))
        {
            if (p.Type == PunchType.In)
            {
                // If they clock in twice, keep the latest as open punch (or you could ignore)
                openIn = p;
            }
            else
            {
                if (openIn is null) continue; // ignore stray OUT

                var start = TimeRounding.RoundToNearestMinutes(openIn.Timestamp, roundingMinutes);
                var end = TimeRounding.RoundToNearestMinutes(p.Timestamp, roundingMinutes);

                if (end > start)
                    shifts.Add(new WorkShift(start, end));

                openIn = null;
            }
        }

        return shifts;
    }

    public OvertimeResult CalculateWeeklyOvertime(IReadOnlyList<WorkShift> shifts, double weeklyThresholdHours)
    {
        var total = shifts.Sum(s => s.Hours);
        var overtime = Math.Max(0, total - weeklyThresholdHours);
        var regular = total - overtime;

        // Keep results stable (avoid 12.00000002 style)
        regular = Math.Round(regular, 2);
        overtime = Math.Round(overtime, 2);

        return new OvertimeResult(regular, overtime);
    }
}
