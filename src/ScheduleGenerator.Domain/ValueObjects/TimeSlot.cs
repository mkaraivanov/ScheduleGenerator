namespace ScheduleGenerator.Domain.ValueObjects;

/// <summary>
/// Represents a time slot with start and end date/time.
/// </summary>
public record TimeSlot
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public TimeSlot(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            throw new ArgumentException("End time must be after start time.", nameof(end));
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the duration of the time slot.
    /// </summary>
    public TimeSpan Duration => End - Start;

    /// <summary>
    /// Checks if this time slot overlaps with another time slot.
    /// </summary>
    public bool OverlapsWith(TimeSlot other)
    {
        return Start < other.End && End > other.Start;
    }

    /// <summary>
    /// Checks if this time slot contains a specific date/time.
    /// </summary>
    public bool Contains(DateTime dateTime)
    {
        return dateTime >= Start && dateTime < End;
    }

    /// <summary>
    /// Gets the time between this slot and another slot (if any).
    /// </summary>
    public TimeSpan? TimeBetween(TimeSlot other)
    {
        if (OverlapsWith(other))
        {
            return null;
        }

        if (End <= other.Start)
        {
            return other.Start - End;
        }

        return Start - other.End;
    }
}
