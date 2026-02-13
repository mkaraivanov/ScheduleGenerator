namespace ScheduleGenerator.Application.Models;

/// <summary>
/// Output DTO representing the complete schedule with diagnostics
/// </summary>
public record ScheduleOutput
{
    public required string TournamentName { get; init; }
    public required List<ScheduledMatchDto> Matches { get; init; }
    public required ScheduleDiagnostics Diagnostics { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// A scheduled match with time and field assignment
/// </summary>
public record ScheduledMatchDto
{
    public required int MatchId { get; init; }
    public required string HomeTeam { get; init; }
    public required string AwayTeam { get; init; }
    public required string Field { get; init; }
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public string? Stage { get; init; }
    public string? Group { get; init; }
    public int? Round { get; init; }
}

/// <summary>
/// Diagnostics information about the schedule
/// </summary>
public record ScheduleDiagnostics
{
    public required bool IsValid { get; init; }
    public required int TotalMatches { get; init; }
    public required int HardConstraintViolations { get; init; }
    public required int SoftConstraintViolations { get; init; }
    public required List<string> Violations { get; init; }
    public required List<string> Warnings { get; init; }
    public TimeSpan? GenerationTime { get; init; }
}
