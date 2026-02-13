namespace ScheduleGenerator.Application.Models;

/// <summary>
/// Main input DTO representing the complete tournament definition
/// </summary>
public record TournamentDefinition
{
    public required string Name { get; init; }
    public required List<TeamDefinition> Teams { get; init; }
    public required List<FieldDefinition> Fields { get; init; }
    public required FormatConfiguration Format { get; init; }
    public required SchedulingRules Rules { get; init; }
    public ConstraintConfiguration? Constraints { get; init; }
}

/// <summary>
/// Team information with optional seeding
/// </summary>
public record TeamDefinition
{
    public required string Name { get; init; }
    public int? Seed { get; init; }
}

/// <summary>
/// Field definition with availability windows
/// </summary>
public record FieldDefinition
{
    public required string Name { get; init; }
    public required List<TimeWindow> AvailabilityWindows { get; init; }
}

/// <summary>
/// Time window representing a period of availability
/// </summary>
public record TimeWindow
{
    public required DateTime Start { get; init; }
    public required DateTime End { get; init; }
}

/// <summary>
/// Tournament format configuration
/// </summary>
public record FormatConfiguration
{
    public required string Type { get; init; } // "RoundRobin", "Groups", "Knockout"
    public GroupStageConfiguration? GroupStage { get; init; }
    public KnockoutConfiguration? Knockout { get; init; }
}

/// <summary>
/// Configuration for group stage tournaments
/// </summary>
public record GroupStageConfiguration
{
    public required int NumberOfGroups { get; init; }
    public int? TeamsAdvancingPerGroup { get; init; }
    public AdvancementConfiguration? Advancement { get; init; }
}

/// <summary>
/// Configuration for knockout stage
/// </summary>
public record KnockoutConfiguration
{
    public bool IncludeThirdPlaceMatch { get; init; }
}

/// <summary>
/// Configuration for team advancement between stages
/// </summary>
public record AdvancementConfiguration
{
    public required string NextStage { get; init; } // "Knockout", etc.
    public required int TeamsAdvancing { get; init; }
}
