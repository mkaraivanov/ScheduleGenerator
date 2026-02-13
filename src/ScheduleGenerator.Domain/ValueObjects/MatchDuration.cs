namespace ScheduleGenerator.Domain.ValueObjects;

/// <summary>
/// Represents the duration of a match including buffer time.
/// </summary>
public record MatchDuration
{
    public int MatchMinutes { get; init; }
    public int BufferMinutes { get; init; }

    public MatchDuration(int matchMinutes, int bufferMinutes = 0)
    {
        if (matchMinutes <= 0)
        {
            throw new ArgumentException("Match minutes must be positive.", nameof(matchMinutes));
        }

        if (bufferMinutes < 0)
        {
            throw new ArgumentException("Buffer minutes cannot be negative.", nameof(bufferMinutes));
        }

        MatchMinutes = matchMinutes;
        BufferMinutes = bufferMinutes;
    }

    /// <summary>
    /// Gets the total duration including match and buffer time.
    /// </summary>
    public int TotalMinutes => MatchMinutes + BufferMinutes;

    /// <summary>
    /// Gets the total duration as a TimeSpan.
    /// </summary>
    public TimeSpan TotalDuration => TimeSpan.FromMinutes(TotalMinutes);

    /// <summary>
    /// Gets the match duration as a TimeSpan.
    /// </summary>
    public TimeSpan MatchTimeSpan => TimeSpan.FromMinutes(MatchMinutes);

    /// <summary>
    /// Gets the buffer duration as a TimeSpan.
    /// </summary>
    public TimeSpan BufferTimeSpan => TimeSpan.FromMinutes(BufferMinutes);

    /// <summary>
    /// Standard match duration (90 minutes match + 15 minutes buffer).
    /// </summary>
    public static MatchDuration Standard => new(90, 15);

    /// <summary>
    /// Short match duration (60 minutes match + 10 minutes buffer).
    /// </summary>
    public static MatchDuration Short => new(60, 10);

    /// <summary>
    /// Youth match duration (70 minutes match + 10 minutes buffer).
    /// </summary>
    public static MatchDuration Youth => new(70, 10);
}
