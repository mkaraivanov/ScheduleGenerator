namespace ScheduleGenerator.Application.Models;

/// <summary>
/// Configuration for soft constraints with weights
/// </summary>
public record ConstraintConfiguration
{
    /// <summary>
    /// Weight for balanced kickoff times across teams (0 = disabled)
    /// </summary>
    public int BalancedKickoffTimesWeight { get; init; } = 1;

    /// <summary>
    /// Weight for minimizing field changes for each team (0 = disabled)
    /// </summary>
    public int MinimizeFieldChangesWeight { get; init; } = 1;

    /// <summary>
    /// Weight for maximizing opponent spacing (0 = disabled)
    /// </summary>
    public int OpponentSpacingWeight { get; init; } = 1;

    /// <summary>
    /// Weight for seeded team separation (0 = disabled)
    /// </summary>
    public int SeededTeamSeparationWeight { get; init; } = 1;

    /// <summary>
    /// Custom weights for specific constraints
    /// </summary>
    public Dictionary<string, int>? CustomWeights { get; init; }
}
