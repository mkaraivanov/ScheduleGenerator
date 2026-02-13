namespace ScheduleGenerator.Application.Models;

/// <summary>
/// Rules and constraints for scheduling
/// </summary>
public record SchedulingRules
{
    /// <summary>
    /// Duration of each match in minutes
    /// </summary>
    public required int MatchDurationMinutes { get; init; }

    /// <summary>
    /// Buffer time between matches on the same field in minutes
    /// </summary>
    public required int BufferBetweenMatchesMinutes { get; init; }

    /// <summary>
    /// Minimum rest time for a team between matches in minutes
    /// </summary>
    public int MinimumRestTimeMinutes { get; init; } = 0;

    /// <summary>
    /// Maximum number of matches a team can play per day
    /// </summary>
    public int? MaxMatchesPerTeamPerDay { get; init; }

    /// <summary>
    /// Minimum time spacing between matches against the same opponent (in minutes)
    /// </summary>
    public int? MinimumOpponentSpacingMinutes { get; init; }
}
