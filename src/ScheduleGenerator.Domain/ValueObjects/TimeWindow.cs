namespace ScheduleGenerator.Domain.ValueObjects;

/// <summary>
/// Represents a time window with day boundaries and blackout periods.
/// </summary>
public record TimeWindow
{
    public TimeOnly DayStart { get; init; }
    public TimeOnly DayEnd { get; init; }
    public IReadOnlyCollection<TimeSlot> BlackoutPeriods { get; init; }

    public TimeWindow(TimeOnly dayStart, TimeOnly dayEnd, IEnumerable<TimeSlot>? blackoutPeriods = null)
    {
        if (dayEnd <= dayStart)
        {
            throw new ArgumentException("Day end time must be after day start time.", nameof(dayEnd));
        }

        DayStart = dayStart;
        DayEnd = dayEnd;
        BlackoutPeriods = blackoutPeriods?.ToList() ?? new List<TimeSlot>();
    }

    /// <summary>
    /// Checks if a given time slot falls within the time window and doesn't overlap with blackout periods.
    /// </summary>
    public bool IsAvailable(TimeSlot timeSlot)
    {
        // Check if the time slot falls within day boundaries
        var slotStartTime = TimeOnly.FromDateTime(timeSlot.Start);
        var slotEndTime = TimeOnly.FromDateTime(timeSlot.End);

        if (slotStartTime < DayStart || slotEndTime > DayEnd)
        {
            return false;
        }

        // Check if the time slot overlaps with any blackout period
        return !BlackoutPeriods.Any(blackout => blackout.OverlapsWith(timeSlot));
    }

    /// <summary>
    /// Gets the total available duration per day (excluding blackout periods).
    /// </summary>
    public TimeSpan GetAvailableDurationPerDay()
    {
        var totalDayDuration = DayEnd - DayStart;
        
        // For simplicity, we assume blackout periods are within a single day
        var blackoutDuration = BlackoutPeriods
            .Sum(b => b.Duration.TotalMinutes);

        return totalDayDuration - TimeSpan.FromMinutes(blackoutDuration);
    }

    /// <summary>
    /// Standard time window: 9 AM to 9 PM with no blackout periods.
    /// </summary>
    public static TimeWindow Standard => new(new TimeOnly(9, 0), new TimeOnly(21, 0));

    /// <summary>
    /// Weekend time window: 8 AM to 8 PM with no blackout periods.
    /// </summary>
    public static TimeWindow Weekend => new(new TimeOnly(8, 0), new TimeOnly(20, 0));
}
